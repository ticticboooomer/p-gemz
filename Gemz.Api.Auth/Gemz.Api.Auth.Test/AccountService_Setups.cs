using Gemz.Api.Auth.Data.Model;
using Gemz.Api.Auth.Service.Accounts.Model;

namespace Gemz.Api.Auth.Test;

public class AccountService_Setups
{
    public string SetupAccountId()
    {
        return Guid.NewGuid().ToString();
    }

    public Account SetupAccountData(string accountId)
    {
        return new Account()
        {
            Id = accountId,
            TwitchUserId = Guid.NewGuid().ToString(),
            TwitchEmail = "twitchemail@email.com",
            EmailAddress = "otheremail@somewhere.com",
            TwitchUsername = "TwitchUserNameTest",
            IsCreator = false,
            TwitchEmailVerified = true,
            Tokens = new Account.TwitchTokens()
            {
                AccessCode = Guid.NewGuid().ToString(),
                RefreshCode = Guid.NewGuid().ToString()
            },
            OnboardingStatus = 3,
            StripeAccountId = "acct_1234567890",
            StripeDetailsSubmitted = true,
            StripeChargesEnabled = true
        };
    }

    public AccountUpdateModel SetupAccountUpdateModel()
    {
        return new AccountUpdateModel()
        {
            EmailAddress = "IamNewemail@testemail.com"
        };
    }

    public Account SetupUpdatedAccount(Account accountData, string updatedEmailAddress)
    {
        return new Account()
        {
            Id = accountData.Id,
            TwitchUserId = accountData.TwitchUserId,
            TwitchUsername = accountData.TwitchUsername,
            TwitchEmail = accountData.TwitchEmail,
            EmailAddress = updatedEmailAddress,
            TwitchEmailVerified = accountData.TwitchEmailVerified,
            IsCreator = accountData.IsCreator,
            Tokens = accountData.Tokens,
            StripeAccountId = accountData.StripeAccountId,
            StripeDetailsSubmitted = accountData.StripeDetailsSubmitted,
            StripeChargesEnabled = accountData.StripeChargesEnabled
        };
    }

    public AccountModel SetupAccountModel(string emailAddress, Account account)
    {
        return new AccountModel()
        {
            EmailAddress = emailAddress,
            IsCreator = account.IsCreator,
            TwitchUsername = account.TwitchUsername,
            OnboardingStatus = 3
        };
    }
}