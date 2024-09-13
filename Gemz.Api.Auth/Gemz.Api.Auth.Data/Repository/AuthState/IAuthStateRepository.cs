namespace Gemz.Api.Auth.Data.Repository.AuthState;

public interface IAuthStateRepository
{
    Task<Model.AuthState> CreateAsync(Model.AuthState entity);
    Task<Model.AuthState> GetAsync(string state);
    Task<Model.AuthState> DeleteAsync(string state);
}