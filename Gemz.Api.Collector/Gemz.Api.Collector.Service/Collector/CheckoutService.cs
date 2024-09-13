using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Data.Repository;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Gemz.Api.Collector.Service.Collector;

public class CheckoutService : ICheckoutService
{
    private readonly IBasketRepository _basketRepo;
    private readonly ICollectionRepository _collectionRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IOptions<StripeConfig> _stripeConfig;
    private readonly IAccountRepository _accountRepo;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(IBasketRepository basketRepo,
        ICollectionRepository collectionRepo,
        IOrderRepository orderRepo,
        IAccountRepository accountRepo,
        IOptions<StripeConfig> stripeConfig, ILogger<CheckoutService> logger)
    {
        _basketRepo = basketRepo;
        _collectionRepo = collectionRepo;
        _orderRepo = orderRepo;
        _stripeConfig = stripeConfig;
        _logger = logger;
        _accountRepo = accountRepo;
    }

    public async Task<GenericResponse<StripePaymentInformation>> CreatePaymentIntent(string collectorId, CreatePIInputModel createPIInputModel)
    {
        _logger.LogDebug("Entered CreatePaymentIntent function.");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Missing Collector Id. Leaving function.");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001000"
            };
        }

        if (string.IsNullOrEmpty(createPIInputModel.OrderId))
        {
            _logger.LogError("Missing order Id. Leaving function.");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001001"
            };
        }

        if (string.IsNullOrEmpty(_stripeConfig.Value.ApiKey))
        {
            _logger.LogError("Stripe Api Key is missing. Leaving function.");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001002"
            };
        }

        var existingOrder = await _orderRepo.FetchOrderByIdAsync(createPIInputModel.OrderId);
        if (existingOrder == null)
        {
            _logger.LogError("Order not found. Leaving function.");
            _logger.LogInformation($"Order Id passed in: {createPIInputModel.OrderId}");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001003"
            };
        }

        if (existingOrder.CollectorId != collectorId)
        {
            _logger.LogError("Order found but not for this collector. Leaving function.");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001004"
            };
        }

        var creatorDetails = await _accountRepo.GetAccountById(existingOrder.CreatorId);
        if (creatorDetails == null)
        {
            _logger.LogError("Repo error on fetch of Creator Details for order. Leaving function.");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001008"
            };
        }

        if (!creatorDetails.IsCreator)
        {
            _logger.LogError("Account that owns the store for this basket is NOT a creator. Leaving function.");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001011"
            };
        }

        if (creatorDetails.OnboardingStatus != (int)OnboardingStatusEnum.Complete
            || creatorDetails.RestrictedStatus != 0)
        {
            _logger.LogError("Cannot checkout this order as Creator Store is either not fully onboarded or is restricted. Leaving function.");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001009"
            };
        }

        if (string.IsNullOrEmpty(creatorDetails.StripeAccountId))
        {
            _logger.LogError("Cannot checkout this order as Creator Stripe AccountId is missing. Leaving function.");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001010"
            };
        }

        StripeConfiguration.ApiKey = _stripeConfig.Value.ApiKey;

        var orderTotalInCents = Convert.ToInt64(existingOrder.OrderTotal * 100);
        var orderCommissionInCents = Convert.ToInt64(existingOrder.CommissionAmount * 100);

        var paymentIntentService = new PaymentIntentService();
        var paymentIntent = await paymentIntentService.CreateAsync((new PaymentIntentCreateOptions()
        {
            Amount = orderTotalInCents,
            Currency = "usd",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions()
            {
                Enabled = true
            },
            ApplicationFeeAmount = orderCommissionInCents,
            Metadata = new Dictionary<string, string>()
            {
                { "orderId", existingOrder.Id },
                { "collectorId", existingOrder.CollectorId }
            }
        }),
            new RequestOptions()
            {
                StripeAccount = creatorDetails.StripeAccountId
            });

        if (paymentIntent is null)
        {
            _logger.LogError("Problem accessing Stripe");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001005"
            };
        }

        if (string.IsNullOrEmpty(paymentIntent.ClientSecret))
        {
            _logger.LogError("Stripe failed to return a Client Secret for the payment Intent");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001006"
            };
        }

        var patchedOrderSuccess = await _orderRepo.PatchPaymentIntentClientSecret
            (existingOrder.Id, paymentIntent.ClientSecret, creatorDetails.StripeAccountId);

        if (!patchedOrderSuccess)
        {
            _logger.LogError("Unable to patch Order with client secret.");
            _logger.LogInformation($"OrderId: {existingOrder.Id}");
            return new GenericResponse<StripePaymentInformation>()
            {
                Error = "CL001007"
            };
        }

        return new GenericResponse<StripePaymentInformation>()
        {
            Data = new StripePaymentInformation()
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentConnectedAccount = creatorDetails.StripeAccountId
            }
        };
    }

    public async Task<GenericResponse<OrderFromBasketOutputModel>> CreateNewOrderFromActiveBasket(string collectorId, OrderFromBasketInputModel orderFromBasketInputModel)
    {
        _logger.LogDebug("Entered CreateNewOrderFromActiveBasket function");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("No collector Id was passed in. Leaving function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001600"
            };
        }

        if (string.IsNullOrEmpty(orderFromBasketInputModel.BasketId))
        {
            _logger.LogError("Missing or empty BasketId parameter. Leaving function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001604"
            };
        }

        var collectorBasket = await _basketRepo.GetActiveBasketById(orderFromBasketInputModel.BasketId);
        if (collectorBasket is null)
        {
            _logger.LogError("Unable to fetch basket for this collector. Leaving Function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001601"
            };
        }

        if (collectorBasket.CollectorId != collectorId)
        {
            _logger.LogError("Basket retrieved is not for CollectorId passed in. Leaving function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001605"
            };
        }

        if (collectorBasket.Items.Count == 0)
        {
            _logger.LogError("No items in collectors Basket. Leaving Function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001602"
            };
        }

        var creatorDetails = await _accountRepo.GetAccountById(collectorBasket.CreatorId);
        if (creatorDetails == null)
        {
            _logger.LogError("Repo error on fetch of Creator Details for basket. Leaving function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001606"
            };
        }

        if (!creatorDetails.IsCreator)
        {
            _logger.LogError("Account that owns the store for this basket is NOT a creator. Leaving function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001610"
            };
        }

        if (creatorDetails.OnboardingStatus != (int)OnboardingStatusEnum.Complete
            || creatorDetails.RestrictedStatus != 0)
        {
            _logger.LogError("Cannot create order as Creator Store is either not fully onboarded or is restricted. Leaving function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001607"
            };
        }

        if (string.IsNullOrEmpty(creatorDetails.StripeAccountId))
        {
            _logger.LogError("Cannot create order as Creator Stripe AccountId is missing. Leaving function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001608"
            };
        }

        if (creatorDetails.CommissionPercentage <= 0)
        {
            _logger.LogError("Creator Commission Percentage is <=0. Leaving function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001609"
            };
        }

        var newOrder = await CreateOrderWithTotals(collectorBasket, creatorDetails.CommissionPercentage);
        if (newOrder is null)
        {
            _logger.LogError("Problem creating new order. Leaving function.");
            return new GenericResponse<OrderFromBasketOutputModel>()
            {
                Error = "CL001603"
            };
        }

        return new GenericResponse<OrderFromBasketOutputModel>
        {
            Data = new OrderFromBasketOutputModel()
            {
                OrderId = newOrder.Id
            }
        };
    }

    public async Task<GenericResponse<OrderFromFailOutputModel>> CreateNewOrderFromFailedOrder(string collectorId, OrderFromFailInputModel orderFromFailInputModel)
    {
        _logger.LogDebug("Entered CreateNewOrderFromFailedOrder function");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("No collector Id was passed in. Leaving funciton.");
            return new GenericResponse<OrderFromFailOutputModel>()
            {
                Error = "CL001700"
            };
        }

        if (string.IsNullOrEmpty(orderFromFailInputModel.FailedOrderId))
        {
            _logger.LogError("No Failed Order Id was passed in. Leaving funciton.");
            return new GenericResponse<OrderFromFailOutputModel>()
            {
                Error = "CL001701"
            };
        }

        var failedOrder = await _orderRepo.FetchOrderByIdAsync(orderFromFailInputModel.FailedOrderId);

        if (failedOrder is null) 
        {
            _logger.LogError("Unable to retrieve Failed Order. Leaving funciton.");
            return new GenericResponse<OrderFromFailOutputModel>()
            {
                Error = "CL001702"
            };
        }

        var updatedSuccess = await _orderRepo.PatchOrderStatus(failedOrder.Id, OrderStatus.Replaced);
        if (!updatedSuccess)
        {
            _logger.LogError("Error occurred during update status of failed order. Leaving function.");
            return new GenericResponse<OrderFromFailOutputModel>()
            {
                Error = "CL001704"
            };
        }

        var newOrder = await CreateOrderWithTotalsFromFailedOrder(failedOrder);
        if (newOrder is null)
        {
            _logger.LogError("Problem creating new order. Leaving function.");
            return new GenericResponse<OrderFromFailOutputModel>()
            {
                Error = "CL001703"
            };
        }

        return new GenericResponse<OrderFromFailOutputModel>
        {
            Data = new OrderFromFailOutputModel()
            {
                OrderId = newOrder.Id
            }
        };
    }


    private async Task<Order> CreateOrderWithTotals(Basket basket, int commissionPercentage)
    {
        var builtOrder = new Order()
        {
            Id = Guid.NewGuid().ToString(),
            OriginatingBasketId = basket.Id,
            Status = OrderStatus.New,
            OrderTotal = (float)0.00,
            CommissionAmount = (float)0.00,
            CollectorId = basket.CollectorId,
            CreatorId = basket.CreatorId,
            PaymentIntentClientSecret = string.Empty,
            StripeErrorMessage = string.Empty,
            Items = new List<OrderItem>(),
            CreatedOn = DateTime.Now,
        };

        var orderTotal = 0.00;
        foreach (var item in basket.Items)
        {
            var lineCollection = await _collectionRepo.GetSingleCollection(item.CollectionId);
            if (lineCollection is null) continue;

            if (lineCollection.CreatorId is null) continue;

            var lineTotal = Math.Round(item.Quantity * lineCollection.ClusterPrice, 2);
            builtOrder.Items.Add(new OrderItem()
            {
                Id = Guid.NewGuid().ToString(),
                Quantity = item.Quantity,
                CollectionId = item.CollectionId,
                PackPrice = lineCollection.ClusterPrice,
                LineTotal = lineTotal
            });

            orderTotal += lineTotal;
        }

        builtOrder.OrderTotal = Math.Round(orderTotal, 2);
        var commissionPercentageValue = Convert.ToDouble(commissionPercentage) / 100;
        builtOrder.CommissionAmount = Math.Round(orderTotal * commissionPercentageValue, 2);

        var newOrder = await _orderRepo.CreateOrderAsync(builtOrder);

        return newOrder ?? null;
    }

    private async Task<Order> CreateOrderWithTotalsFromFailedOrder(Order order)
    {
        var builtOrder = new Order()
        {
            Id = Guid.NewGuid().ToString(),
            OriginatingBasketId = order.OriginatingBasketId,
            Status = OrderStatus.New,
            OrderTotal = order.OrderTotal,
            CommissionAmount = order.CommissionAmount,
            CollectorId = order.CollectorId,
            CreatorId = order.CreatorId,
            PaymentIntentClientSecret = string.Empty,
            StripeErrorMessage = string.Empty,
            Items = new List<OrderItem>(),
            CreatedOn = order.CreatedOn,
        };

        foreach (var item in order.Items)
        {
            var lineCollection = await _collectionRepo.GetSingleCollection(item.CollectionId);
            if (lineCollection is null) continue;

            if (lineCollection.CreatorId is null) continue;

            var lineTotal = Math.Round(item.Quantity * lineCollection.ClusterPrice, 2);
            builtOrder.Items.Add(new OrderItem()
            {
                Id = Guid.NewGuid().ToString(),
                Quantity = item.Quantity,
                CollectionId = item.CollectionId,
                PackPrice = item.PackPrice,
                LineTotal = item.LineTotal
            });
        }

        var newOrder = await _orderRepo.CreateOrderAsync(builtOrder);

        return newOrder ?? null;

    }
}