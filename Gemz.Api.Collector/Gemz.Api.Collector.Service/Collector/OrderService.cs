using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Data.Repository;
using Gemz.Api.Collector.Service.Collector.Model;
using Gemz.ServiceBus.Factory;
using Gemz.ServiceBus.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Gemz.Api.Collector.Service.Collector;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly ICollectionRepository _collectionRepo;
    private readonly IStoreTagRepository _storeTagRepo;
    private readonly IStripePaymentIntentRepository _stripePaymentIntentRepo;
    private readonly IBasketRepository _basketRepo;
    private readonly ICollectorPackRepository _collectorPackRepo;
    private readonly ICollectorPackOpenedRepository _collectorPackOpenedRepo;
    private readonly IOptions<ServiceBusConfig> _serviceBusConfig;
    private readonly IAccountRepository _accountRepo;
    private readonly ILogger<OrderService> _logger;
    private readonly ISendEndpointProvider _endpointProvider;
    private readonly IStoreRepository _storeRepo;

    public OrderService(IOrderRepository orderRepo, ICollectionRepository collectionRepo,
        IStoreTagRepository storeTagRepo,
        IStripePaymentIntentRepository stripePaymentIntentRepo,
        IBasketRepository basketRepo,
        ICollectorPackRepository collectorPackRepo,
        ICollectorPackOpenedRepository collectorPackOpenedRepo,
        IOptions<ServiceBusConfig> serviceBusConfig,
        IAccountRepository accountRepo,
        ILogger<OrderService> logger,
        ISendEndpointProvider endpointProvider, IStoreRepository storeRepo)
    {
        _orderRepo = orderRepo;
        _collectionRepo = collectionRepo;
        _storeTagRepo = storeTagRepo;
        _stripePaymentIntentRepo = stripePaymentIntentRepo;
        _basketRepo = basketRepo;
        _collectorPackRepo = collectorPackRepo;
        _collectorPackOpenedRepo = collectorPackOpenedRepo;
        _serviceBusConfig = serviceBusConfig;
        _accountRepo = accountRepo;
        _logger = logger;
        _endpointProvider = endpointProvider;
        _storeRepo = storeRepo;
    }

    public async Task<GenericResponse<OrderOutputModel>> FetchOrderById(string collectorId, OrderIdModel orderIdModel)
    {
        _logger.LogDebug("Entered FetchOrderById function.");
        _logger.LogInformation($"collector: {collectorId} | orderId: {orderIdModel.OrderId}");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Collector Id parameter is missing or empty. Leaving Function.");
            return new GenericResponse<OrderOutputModel>()
            {
                Error = "CL001100"
            };
        }

        if (string.IsNullOrEmpty(orderIdModel.OrderId))
        {
            _logger.LogError("OrderId parameter is missing or empty. Leaving function.");
            return new GenericResponse<OrderOutputModel>()
            {
                Error = "CL001101"
            };
        }

        var order = await _orderRepo.FetchOrderByIdAsync(orderIdModel.OrderId);

        if (order is null)
        {
            _logger.LogError("Unable to find order. Leaving function.");
            return new GenericResponse<OrderOutputModel>()
            {
                Error = "CL001102"
            };
        }

        if (order.CollectorId != collectorId)
        {
            _logger.LogError("Order fetched is not for this collector. Leaving Function.");
            return new GenericResponse<OrderOutputModel>()
            {
                Error = "CL001103"
            };
        }

        var orderOutputModel = await MapOrderToOrderOutputModel(order);

        if (orderOutputModel is null)
        {
            _logger.LogError(
                "There was a problem with the collection or store within the order lines. Leaving function.");
            return new GenericResponse<OrderOutputModel>()
            {
                Error = "CL001104"
            };
        }

        return new GenericResponse<OrderOutputModel>()
        {
            Data = orderOutputModel
        };
    }

    public async Task<GenericResponse<OrderPisOutputModel>> FetchOrderByPaymentIntentSecret(string collectorId,
        PaymentIntentInputModel paymentIntentInputModel)
    {
        _logger.LogDebug("Entered FetchOrderByPaymentIntentSecret function.");
        _logger.LogInformation(
            $"collector: {collectorId} | PaymentIntentSecret: {paymentIntentInputModel.PaymentIntentSecret}");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Collector Id parameter is missing or empty. Leaving Function.");
            return new GenericResponse<OrderPisOutputModel>()
            {
                Error = "CL001200"
            };
        }

        if (string.IsNullOrEmpty(paymentIntentInputModel.PaymentIntentSecret))
        {
            _logger.LogError("PaymentIntentSecret parameter is missing or empty. Leaving function.");
            return new GenericResponse<OrderPisOutputModel>()
            {
                Error = "CL001201"
            };
        }

        var order = await _orderRepo.FetchOrderByPaymentIntentSecretAsync(paymentIntentInputModel.PaymentIntentSecret);

        if (order is null)
        {
            _logger.LogError("Unable to find order. Leaving function.");
            return new GenericResponse<OrderPisOutputModel>()
            {
                Error = "CL001202"
            };
        }

        if (order.CollectorId != collectorId)
        {
            _logger.LogError("Order fetched is not for this collector. Leaving Function.");
            return new GenericResponse<OrderPisOutputModel>()
            {
                Error = "CL001203"
            };
        }

        var orderPisOutputModel = await MapOrderToOrderPisOutputModel(order);

        if (orderPisOutputModel is null)
        {
            _logger.LogError(
                "There was a problem with the collection or storetag within the order lines. Leaving function.");
            return new GenericResponse<OrderPisOutputModel>()
            {
                Error = "CL001204"
            };
        }

        return new GenericResponse<OrderPisOutputModel>()
        {
            Data = orderPisOutputModel
        };
    }


    public async Task<GenericResponse<OrderStatusOutputModel>> UpdateOrderStatus(string collectorId,
        OrderStatusInputModel orderStatusInputModel)
    {
        _logger.LogDebug("Entered UpdateOrderStatus function.");
        _logger.LogInformation(
            $"collector: {collectorId} | SearchKeyData: {orderStatusInputModel.SearchKeyData} | SearchKeyUsed: {orderStatusInputModel.SearchKeyUsed}");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Collector Id parameter is missing or empty. Leaving Function.");
            return new GenericResponse<OrderStatusOutputModel>()
            {
                Error = "CL001300"
            };
        }

        if (string.IsNullOrEmpty(orderStatusInputModel.SearchKeyData))
        {
            _logger.LogError("Search Key Data parameter is missing or empty. Leaving function.");
            return new GenericResponse<OrderStatusOutputModel>()
            {
                Error = "CL001301"
            };
        }

        if (string.IsNullOrEmpty(orderStatusInputModel.SearchKeyUsed))
        {
            _logger.LogError("SearchKeyUsed parameter is missing or empty. Leaving function.");
            return new GenericResponse<OrderStatusOutputModel>()
            {
                Error = "CL001302"
            };
        }

        if (orderStatusInputModel.SearchKeyUsed.ToUpper() != "ID" &&
            orderStatusInputModel.SearchKeyUsed.ToUpper() != "PIS")
        {
            _logger.LogError("SearchKeyUsed parameter is not set to a valid ket identifier. Leaving function.");
            return new GenericResponse<OrderStatusOutputModel>()
            {
                Error = "CL001303"
            };
        }


        if (!Enum.IsDefined(typeof(OrderStatus), orderStatusInputModel.NewOrderStatus))
        {
            _logger.LogError("NewOrderStatus parameter is not set to a valid status. Leaving function.");
            return new GenericResponse<OrderStatusOutputModel>()
            {
                Error = "CL001306"
            };

        }

        Order? order;
        if (orderStatusInputModel.SearchKeyUsed.ToUpper() == "ID")
        {
            order = await _orderRepo.FetchOrderByIdAsync(orderStatusInputModel.SearchKeyData);
        }
        else
        {
            order = await _orderRepo.FetchOrderByPaymentIntentSecretAsync(orderStatusInputModel.SearchKeyData);
        }

        if (order is null)
        {
            _logger.LogError("Unable to find order. Leaving function.");
            return new GenericResponse<OrderStatusOutputModel>()
            {
                Error = "CL001304"
            };
        }

        if (order.CollectorId != collectorId)
        {
            _logger.LogWarning("Order fetched is not for this collector. Leaving Function.");
            return new GenericResponse<OrderStatusOutputModel>()
            {
                Error = "CL001305"
            };
        }

        var updatedOrderSuccess = await _orderRepo.PatchOrderStatus(order.Id, (OrderStatus)orderStatusInputModel.NewOrderStatus);

        if (!updatedOrderSuccess)
        {
            _logger.LogError("there was a problem patching the order status. Leaving function.");
            return new GenericResponse<OrderStatusOutputModel>()
            {
                Error = "CL001307"
            };
        }

        order.Status = (OrderStatus)orderStatusInputModel.NewOrderStatus;

        var orderStatusOutputModel = await MapOrderToOrderStatusOutputModel(order);

        if (orderStatusOutputModel is null)
        {
            _logger.LogError(
                "There was a problem with the collection or storetag within the order lines. Leaving function.");
            return new GenericResponse<OrderStatusOutputModel>()
            {
                Error = "CL001308"
            };
        }

        return new GenericResponse<OrderStatusOutputModel>()
        {
            Data = orderStatusOutputModel
        };
    }



    public async Task<GenericResponse<PaymentPendingOutputModel>> UpdateOrderForPaymentPending(string collectorId, PaymentPendingInputModel paymentPendingInputModel)
    {
        _logger.LogDebug("Entered UpdateOrderForPaymentPending function.");
        _logger.LogInformation(
            $"collector: {collectorId} | PIS: {paymentPendingInputModel.PaymentIntentClientSecret}");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Collector Id parameter is missing or empty. Leaving Function.");
            return new GenericResponse<PaymentPendingOutputModel>()
            {
                Error = "CL001500"
            };
        }

        if (string.IsNullOrEmpty(paymentPendingInputModel.PaymentIntentClientSecret))
        {
            _logger.LogError("PIS parameter is missing or empty. Leaving function.");
            return new GenericResponse<PaymentPendingOutputModel>()
            {
                Error = "CL001501"
            };
        }


        var existingorder = await _orderRepo.FetchOrderByPaymentIntentSecretAsync(paymentPendingInputModel.PaymentIntentClientSecret);

        if (existingorder is null)
        {
            _logger.LogError("Unable to find order. Leaving function.");
            return new GenericResponse<PaymentPendingOutputModel>()
            {
                Error = "CL001502"
            };
        }

        if (existingorder.CollectorId != collectorId)
        {
            _logger.LogError("Order fetched is not for this collector. Leaving Function.");
            return new GenericResponse<PaymentPendingOutputModel>()
            {
                Error = "CL001503"
            };
        }

        var updatedOrder = existingorder;
        if (existingorder.Status == OrderStatus.New)
        {
            var updatedOrderSuccess = await _orderRepo.PatchOrderStatus(existingorder.Id, OrderStatus.Pending);

            if (!updatedOrderSuccess)
            {
                _logger.LogError("There was a problem patching the order status. Leaving function.");
                return new GenericResponse<PaymentPendingOutputModel>()
                {
                    Error = "CL001504"
                };
            }
            updatedOrder.Status = OrderStatus.Pending;
        }

        if (!string.IsNullOrEmpty(paymentPendingInputModel.StripeErrorMessage))
        {
            var updatedOrderSuccess = await _orderRepo.PatchOrderStripeErrorMessage(existingorder.Id, paymentPendingInputModel.StripeErrorMessage);

            if (!updatedOrderSuccess)
            {
                _logger.LogError("There was a problem patching the order Stripe Error Message. Leaving function.");
                _logger.LogInformation($"Stripe Error Message: {paymentPendingInputModel.StripeErrorMessage}");
                return new GenericResponse<PaymentPendingOutputModel>()
                {
                    Error = "CL001507"
                };
            }
            updatedOrder.StripeErrorMessage = paymentPendingInputModel.StripeErrorMessage;
        }

        var updatedBasket = await _basketRepo.DeactivateBasket(existingorder.OriginatingBasketId);

        if (!updatedBasket)
        {
            _logger.LogError("Error occurred when deactivating basket. Leaving function");
            return new GenericResponse<PaymentPendingOutputModel>()
            {
                Error = "CL001505"
            };
        }


        var paymentPendingOutputModel = await MapOrderToPaymentPendingOutputModel(updatedOrder);

        if (paymentPendingOutputModel is null)
        {
            _logger.LogError(
                "There was a problem with the collection or storetag within the order lines. Leaving function.");
            return new GenericResponse<PaymentPendingOutputModel>()
            {
                Error = "CL001506"
            };
        }

        return new GenericResponse<PaymentPendingOutputModel>()
        {
            Data = paymentPendingOutputModel
        };
    }

    public async Task<GenericResponse<List<OrderHeaderOutputModel>>> FetchOrderList(string collectorId)
    {
        _logger.LogDebug("Entered FetchOrderList function");
        _logger.LogInformation($"CollectorId: {collectorId}");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Missing or Empty Collector Id Parameter. Leaving function.");
            return new GenericResponse<List<OrderHeaderOutputModel>>
            {
                Error = "CL002600"
            };
        }

        var orderList = await _orderRepo.FetchOrdersForCollector(collectorId);

        if (orderList is null)
        {
            _logger.LogError("Repo error during fetch of collector orders. Leaving function.");
            return new GenericResponse<List<OrderHeaderOutputModel>>
            {
                Error = "CL002601"
            };
        }

        return new GenericResponse<List<OrderHeaderOutputModel>>
        {
            Data = await BuildOutputOrderListModel(orderList)
        };
    }

    public async Task<GenericResponse<OrderListPagedOutputModel>> FetchOrderListPaged(string collectorId,
        OrderListPagedInputModel orderListPagedInputModel)
    {
        _logger.LogDebug("Entered FetchOrderListPaged function.");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Missing or empty CollectorId Parameter. Leaving function.");
            return new GenericResponse<OrderListPagedOutputModel>()
            {
                Error = "CL003800"
            };
        }

        if (orderListPagedInputModel.CurrentPage < 0)
        {
            _logger.LogError("CurrentPage parameter is invalid ( <0 ). leaving function.");
            return new GenericResponse<OrderListPagedOutputModel>()
            {
                Error = "CL003801"
            };
        }

        if (orderListPagedInputModel.PageSize <= 0)
        {
            _logger.LogError("PageSize parameter is invalid ( <=0 ). Leaving function.");
            return new GenericResponse<OrderListPagedOutputModel>()
            {
                Error = "CL003802"
            };
        }

        var orderPage = await _orderRepo.GetPageOfOrdersForCollector(collectorId, orderListPagedInputModel.CurrentPage,
            orderListPagedInputModel.PageSize);
        if (orderPage is null)
        {
            _logger.LogError("Repo error during fetch of page of Orders. Leaving function.");
            return new GenericResponse<OrderListPagedOutputModel>()
            {
                Error = "CL003803"
            };
        }

        return new GenericResponse<OrderListPagedOutputModel>()
        {
            Data = new OrderListPagedOutputModel()
            {
                Orders = await BuildOutputOrderListModel(orderPage.Orders),
                ThisPage = orderPage.ThisPage,
                TotalPages = orderPage.TotalPages
            }
        };
    }

    public async Task UpdateOrderStatusFromStripeEvent(PaymentIntentMessageModel paymentIntentMessage)
    {
        _logger.LogDebug("Entered UpdateOrderStatusFromStripeEvent");
        var stripePaymentIntent = new StripePaymentIntent()
        {
            Id = Guid.NewGuid().ToString(),
            PaymentIntentId = paymentIntentMessage.PaymentIntentId,
            Amount = paymentIntentMessage.Amount,
            ApplicationFeeAmount = paymentIntentMessage.ApplicationFeeAmount,
            Currency = paymentIntentMessage.Currency,
            EventType = paymentIntentMessage.EventType,
            LatestChargeId = paymentIntentMessage.LatestChargeId,
            PaymentMethodId = paymentIntentMessage.PaymentMethodId,
            MetadataCollectorId = paymentIntentMessage.MetadataCollectorId,
            MetadataOrderId = paymentIntentMessage.MetadataOrderId,
            PaymentIntentCreatedOn = paymentIntentMessage.CreatedOn,
            CreatedOn = DateTime.UtcNow
        };

        var response = await _stripePaymentIntentRepo.CreateAsync(stripePaymentIntent);

        _logger.LogInformation($"Stripe metadata OrderId: {paymentIntentMessage.MetadataOrderId}");


        if (string.IsNullOrEmpty(paymentIntentMessage.MetadataOrderId))
        {
            _logger.LogError("Stripe Payment Intent Object is missing or empty orderId metadata");
            return;
        }

        if (string.IsNullOrEmpty(paymentIntentMessage.MetadataCollectorId))
        {
            _logger.LogError("Stripe Payment Intent Object is missing or empty collectorId metadata");
            return;
        }
        var existingOrder = await _orderRepo.FetchOrderByIdAsync(paymentIntentMessage.MetadataOrderId);

        if (existingOrder is null)
        {
            _logger.LogError("orderId referenced in Stripe Payment Intent Object is not found in Repo");
            return;
        }

        if (existingOrder.CollectorId != paymentIntentMessage.MetadataCollectorId)
        {
            _logger.LogError("orderId referenced in Stripe Payment Intent is not for the collectorId referenced in metadata");
            return;
        }

        if (existingOrder.PaymentIntentClientSecret != paymentIntentMessage.PaymentIntentClientSecret)
        {
            _logger.LogError("Payment Intent Client Secret does not match the one on the order");
            return;
        }

        if (paymentIntentMessage.EventType == Events.PaymentIntentSucceeded && existingOrder.Status == OrderStatus.Completed)
        {
            _logger.LogDebug("Order already marked as completed. Leaving function.");
            _logger.LogInformation($"Order Id: {paymentIntentMessage.MetadataOrderId} | eventType: {paymentIntentMessage.EventType}");
            return;
        }

        if (paymentIntentMessage.EventType == Events.PaymentIntentPaymentFailed && existingOrder.Status == OrderStatus.PaymentFailed)
        {
            _logger.LogDebug("Order already marked as Payment Failed. Leaving function.");
            _logger.LogInformation($"Order Id: {paymentIntentMessage.MetadataOrderId} | eventType: {paymentIntentMessage.EventType}");
            return;
        }

        bool updatedOrderSuccess;
        if (paymentIntentMessage.EventType == Events.PaymentIntentSucceeded)
        {
            _logger.LogDebug("Updating order status to Completed");
            updatedOrderSuccess = await _orderRepo.PatchOrderStatus(paymentIntentMessage.MetadataOrderId, OrderStatus.Completed);
            existingOrder.Status = OrderStatus.Completed;
        }
        else
        {
            if (paymentIntentMessage.EventType == Events.PaymentIntentPaymentFailed)
            {
                _logger.LogDebug("Updating order status to Payment Failed");
                updatedOrderSuccess = await _orderRepo.PatchOrderStatus(paymentIntentMessage.MetadataOrderId, OrderStatus.PaymentFailed);
                existingOrder.Status = OrderStatus.PaymentFailed;
            }
            else
            {
                updatedOrderSuccess = false;
            }
        }

        if (!updatedOrderSuccess)
        {
            _logger.LogError("Repo error during update of Order Status");
            _logger.LogInformation($"Order Id: {paymentIntentMessage.MetadataOrderId} | eventType: {paymentIntentMessage.EventType}");
            return;
        }

        if (paymentIntentMessage.EventType == Events.PaymentIntentSucceeded)
        {
            var createdPacks = await CreateCollectorPacks(existingOrder);

            if (!createdPacks)
            {
                _logger.LogWarning("There was a problem creating the collector_packs records.");
                return;
            }
        }

        _logger.LogDebug("Successfully updated order status");
    }

    private async Task<bool> CreateCollectorPacks(Order order)
    {
        _logger.LogDebug("Entered CreateCollectorPacks function.");
        var collectorAccount = await _accountRepo.GetAccountById(order.CollectorId);

        foreach (var item in order.Items)
        {
            var orderLineAlreadyHasPack = await CheckIfOrderLineHasHadPacksCreated(order.Id, item.Id);
            if (orderLineAlreadyHasPack)
            {
                _logger.LogError("This order line has already had a pack created for it. Skipping Line.");
                _logger.LogInformation($"OrderId: {order.Id}  |  OrderLineId: {item.Id}");
                continue;
            }

            var collectionData = await _collectionRepo.GetSingleCollectionAnyStatus(item.CollectionId);
            if (collectionData is null)
            {
                _logger.LogError("Repo Error during fetch of collection data. Leaving function.");
                return false;
            }

            _logger.LogInformation($"Creating CollectorPack for orderId: {order.Id} | LineId: {item.Id}");
            for (var i = 0; i <= item.Quantity - 1; i++)
            {
                var collectionPack = new CollectorPack
                {
                    Id = Guid.NewGuid().ToString(),
                    CollectorId = order.CollectorId,
                    OriginatingOrderId = order.Id,
                    OriginatingOrderLineId = item.Id,
                    CreatorId = collectionData.CreatorId,
                    CollectionId = item.CollectionId,
                    CreatedOn = DateTime.UtcNow
                };

                var createdCollectorPack = await _collectorPackRepo.CreateAsync(collectionPack);
                if (createdCollectorPack == null)
                {
                    _logger.LogWarning("Error occurred during CollectorPack create");
                    return false;
                }
                _logger.LogInformation($"Created CollectionPack: {createdCollectorPack.Id}");
            }

            await PushOrderNotificationToBus(new NotifyOrderModel()
            {
                Packs = item.Quantity,
                CreatorId = collectionData.CreatorId,
                CollectorName = collectorAccount.TwitchUsername
            });
        }

        _logger.LogDebug("Completed creating CollectorPacks from Order");
        return true;
    }

    private async Task<bool> CheckIfOrderLineHasHadPacksCreated(string orderId, string lineId)
    {
        var collectorPackForLine = await _collectorPackRepo.FetchPackForOrderLineAsync(orderId, lineId);
        if (collectorPackForLine != null)
        {
            return true;
        }

        var openedCollectorPackForLine = await _collectorPackOpenedRepo.FetchOpenedPackForOrderLineAsync(orderId, lineId);
        if (openedCollectorPackForLine != null)
        {
            return true;
        }

        return false;
    }

    private async Task<List<OrderHeaderOutputModel>> BuildOutputOrderListModel(List<Order> orderList)
    {
        var sortedOrderList = orderList.OrderByDescending(x => x.CreatedOn).ToList();
        var orderOutputList = new List<OrderHeaderOutputModel>();

        foreach (var order in sortedOrderList)
        {
            var storeDetails = await _storeRepo.GetByCreatorId(order.CreatorId);
            if (storeDetails is null)
            {
                _logger.LogWarning("Order could not be listed as creator store not found. Skipping Order.");
                _logger.LogInformation($"order Id: {order.Id}  |  creatorId: {order.CreatorId}");
                continue;
            }

            var formattedDateAndTime = FormatDateAndTime(order.CreatedOn);

            orderOutputList.Add(new OrderHeaderOutputModel
            {
                OrderDate = formattedDateAndTime.GetValueOrDefault("dateOnly"),
                OrderTime = formattedDateAndTime.GetValueOrDefault("timeOnly"),
                OrderId = order.Id,
                OrderTotal = order.OrderTotal,
                Status = order.Status,
                StoreName = storeDetails.Name,
                StoreLogoImageId = storeDetails.LogoImageId,
                StoreTag = storeDetails.UrlStoreTag
            });
        }

        return orderOutputList;
    }

    private static Dictionary<string, string> FormatDateAndTime(DateTime dateAndTime)
    {
        return new Dictionary<string, string>
        {
            { "dateOnly", dateAndTime.ToString("ddd, dd MMM yyyy") },
            { "timeOnly", dateAndTime.ToString("HH:mm 'UTC'") }
        };
    }

    private async Task<OrderStatusOutputModel> MapOrderToOrderStatusOutputModel(Order order)
    {
        var orderStatusOutputModel = new OrderStatusOutputModel()
        {
            OrderId = order.Id,
            CollectorId = order.CollectorId,
            OrderTotal = order.OrderTotal,
            Status = order.Status,
            OriginatingBasketId = order.OriginatingBasketId,
            PaymentIntentClientSecret = order.PaymentIntentClientSecret,
            Items = new List<OrderStatusItemOutputModel>()
        };

        foreach (var item in order.Items)
        {
            var lineCollection = await _collectionRepo.GetSingleCollection(item.CollectionId);
            if (lineCollection is null) return null;

            if (lineCollection.CreatorId is null) return null;

            var storeTag = await _storeTagRepo.GetByCreatorIdAsync(lineCollection.CreatorId);

            if (storeTag is null) return null;

            orderStatusOutputModel.Items.Add(new OrderStatusItemOutputModel()
            {
                OrderLineId = item.Id,
                StoreTag = storeTag.Tagword,
                CollectionId = item.CollectionId,
                CollectionName = lineCollection.Name,
                LineTotal = item.LineTotal,
                PackPrice = item.PackPrice,
                Quantity = item.Quantity
            });
        }

        return orderStatusOutputModel;
    }

    private async Task<OrderOutputModel> MapOrderToOrderOutputModel(Order order)
    {
        var creatorStore = await _storeRepo.GetByCreatorId(order.CreatorId);
        if (creatorStore is null)
        {
            _logger.LogError("Repo error during fetch of Store Details. Leaving function.");
            return null;
        }

        var orderOutputModel = new OrderOutputModel()
        {
            OrderId = order.Id,
            CollectorId = order.CollectorId,
            OrderTotal = order.OrderTotal,
            Status = order.Status,
            OriginatingBasketId = order.OriginatingBasketId,
            PaymentIntentClientSecret = order.PaymentIntentClientSecret,
            StripeErrorMessage = order.StripeErrorMessage,
            OrderDate = FormatDateAndTime(order.CreatedOn).GetValueOrDefault("dateOnly"),
            OrderTime = FormatDateAndTime(order.CreatedOn).GetValueOrDefault("timeOnly"),
            StoreTag = creatorStore.UrlStoreTag,
            StoreName = creatorStore.Name,
            StoreLogoImageId = creatorStore.LogoImageId,
            Items = new List<OrderOutputItem>()
        };

        foreach (var item in order.Items)
        {
            var lineCollection = await _collectionRepo.GetSingleCollection(item.CollectionId);

            if (lineCollection?.CreatorId is null) return null;

            orderOutputModel.Items.Add(new OrderOutputItem()
            {
                OrderLineId = item.Id,
                CollectionId = item.CollectionId,
                CollectionName = lineCollection.Name,
                LineTotal = item.LineTotal,
                PackPrice = item.PackPrice,
                Quantity = item.Quantity
            });
        }

        return orderOutputModel;
    }

    private async Task<OrderPisOutputModel> MapOrderToOrderPisOutputModel(Order order)
    {
        var orderPisOutputModel = new OrderPisOutputModel()
        {
            OrderId = order.Id,
            CollectorId = order.CollectorId,
            OrderTotal = order.OrderTotal,
            Status = order.Status,
            OriginatingBasketId = order.OriginatingBasketId,
            PaymentIntentClientSecret = order.PaymentIntentClientSecret,
            Items = new List<OrderItemPisOutputModel>()
        };

        foreach (var item in order.Items)
        {
            var lineCollection = await _collectionRepo.GetSingleCollection(item.CollectionId);
            if (lineCollection is null) return null;

            if (lineCollection.CreatorId is null) return null;

            var storeTag = await _storeTagRepo.GetByCreatorIdAsync(lineCollection.CreatorId);

            if (storeTag is null) return null;

            orderPisOutputModel.Items.Add(new OrderItemPisOutputModel()
            {
                OrderLineId = item.Id,
                StoreTag = storeTag.Tagword,
                CollectionId = item.CollectionId,
                CollectionName = lineCollection.Name,
                LineTotal = item.LineTotal,
                PackPrice = item.PackPrice,
                Quantity = item.Quantity
            });
        }

        return orderPisOutputModel;
    }


    private async Task<PaymentPendingOutputModel> MapOrderToPaymentPendingOutputModel(Order order)
    {
        var paymentPendingOutputModel = new PaymentPendingOutputModel()
        {
            OrderId = order.Id,
            CollectorId = order.CollectorId,
            OrderTotal = order.OrderTotal,
            Status = order.Status,
            OriginatingBasketId = order.OriginatingBasketId,
            PaymentIntentClientSecret = order.PaymentIntentClientSecret,
            Items = new List<PaymentPendingItemOutputModel>()
        };

        foreach (var item in order.Items)
        {
            var lineCollection = await _collectionRepo.GetSingleCollection(item.CollectionId);
            if (lineCollection is null) return null;

            if (lineCollection.CreatorId is null) return null;

            var storeTag = await _storeTagRepo.GetByCreatorIdAsync(lineCollection.CreatorId);

            if (storeTag is null) return null;

            paymentPendingOutputModel.Items.Add(new PaymentPendingItemOutputModel()
            {
                OrderLineId = item.Id,
                StoreTag = storeTag.Tagword,
                CollectionId = item.CollectionId,
                CollectionName = lineCollection.Name,
                LineTotal = item.LineTotal,
                PackPrice = item.PackPrice,
                Quantity = item.Quantity
            });
        }

        return paymentPendingOutputModel;
    }

    private async Task PushOrderNotificationToBus(NotifyOrderModel model)
    {
        var sender = await _endpointProvider.GetSendEndpoint(new Uri(_serviceBusConfig.Value.NotifyOrderQueueName));
        await sender.Send(model);
    }
}