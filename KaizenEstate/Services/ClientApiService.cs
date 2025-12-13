using System.Net.Http.Json;
using KaizenEstate.Shared.Models;
using Microsoft.Maui.Storage;

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
            return await _httpClient.GetFromJsonAsync<Apartment>($"api/apartments/{id}");
        }

        public async Task<bool> CreateApplicationAsync(EstateApplication application)
        {
            var response = await _httpClient.PostAsJsonAsync("api/applications", application);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<EstateApplication>> GetApplicationsByUserIdAsync(int userId)
        {
            return await _httpClient.GetFromJsonAsync<List<EstateApplication>>($"api/applications/user/{userId}")
                   ?? new List<EstateApplication>();
        }

        public async Task<bool> CreateApartmentAsync(Apartment apartment, FileResult? file)
        {
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(apartment.Title ?? ""), "title");
            content.Add(new StringContent(apartment.Address ?? ""), "address");
            content.Add(new StringContent(apartment.Description ?? ""), "description");
            content.Add(new StringContent(apartment.Price.ToString()), "price");
            content.Add(new StringContent(apartment.Rooms.ToString()), "rooms");
            content.Add(new StringContent(apartment.Area.ToString()), "area");

            if (file != null)
            {
                var stream = await file.OpenReadAsync();
                var fileContent = new StreamContent(stream);
                content.Add(fileContent, "image", file.FileName);
            }

            var response = await _httpClient.PostAsync("api/apartments", content);
            return response.IsSuccessStatusCode;
        }

        // === МЕТОД УДАЛЕНИЯ ===
        public async Task DeleteApartmentAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/apartments/{id}");
        }
    }
}