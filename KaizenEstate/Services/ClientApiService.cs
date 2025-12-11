using System.Net.Http.Json;
using KaizenEstate.Shared.Models;

namespace KaizenEstate.Services
{
    public class ClientApiService
    {
        private readonly HttpClient _httpClient;

        public ClientApiService(HttpClient httpClient)
        {
            _httpClient = httpClient; 
        }

        public async Task<List<Apartment>> GetApartmentsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Apartment>>("api/apartments") ?? new List<Apartment>();
        }
        public async Task<Apartment?> GetApartmentByIdAsync(int id)
        {
            try
            {
                // Запрашиваем конкретную квартиру: api/apartments/5
                return await _httpClient.GetFromJsonAsync<Apartment>($"api/apartments/{id}");
            }
            catch
            {
                return null;
            }
        }
    }
}