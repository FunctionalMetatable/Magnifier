﻿using Blazored.LocalStorage;
using Magnifier.Models;
using Spyglass.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Spyglass.Services
{
    public class AuthenticationService
    {
        private AppSettings AppSettings;
        private HttpClient Http;
        private ISyncLocalStorageService LocalStorage;

        public User user;

        public string token;

        public AuthenticationService(AppSettings _AppSettings, HttpClient _Http, ISyncLocalStorageService _LocalStorage)
        {
            AppSettings = _AppSettings;
            Http = _Http;
            LocalStorage = _LocalStorage;
        }

        public void Initialize()
        {
            user = LocalStorage.GetItem<User>("user");
            token = LocalStorage.GetItem<string>("token");
        }

        public async Task<AuthenticationResponse> Login(string authCode)
        {
            HttpResponseMessage response = await Http.GetAsync($"{AppSettings.ApiRoot}/Auth/token?code={authCode}");

            if (!response.IsSuccessStatusCode)
            {
                return new AuthenticationResponse(response, null);
            }

            token = await response.Content.ReadAsStringAsync();

            LocalStorage.SetItem("token", token);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{AppSettings.ApiRoot}/Auth/user");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LocalStorage.GetItem<string>("token"));
            HttpResponseMessage userResponse = await Http.SendAsync(request);
            string json = await userResponse.Content.ReadAsStringAsync();

            user = System.Text.Json.JsonSerializer.Deserialize<User>(json);

            LocalStorage.SetItem("user", user);

            return new AuthenticationResponse(response, token);
        }

        public void Logout()
        {
            token = null;
            LocalStorage.SetItem("token", token);

            user = null;
            LocalStorage.SetItem("user", user);
        }
    }
}
