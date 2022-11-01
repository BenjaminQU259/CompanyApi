﻿using CompanyApi.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CompanyApiTest.Controllers
{
  public class CompanyControllerTest
  {
    [Fact]
    public async void Should_add_new_company_successfully()
    {
      // given
      var application = new WebApplicationFactory<Program>();
      var httpClient = application.CreateClient();
      await httpClient.DeleteAsync("/companies");
      var company = new Company("SLB");
      var companyJson = JsonConvert.SerializeObject(company);
      var postBody = new StringContent(companyJson, Encoding.UTF8, "application/json");
      // when
      var response = await httpClient.PostAsync("/companies", postBody);
      // then
      Assert.Equal(HttpStatusCode.Created, response.StatusCode);
      var responseBody = await response.Content.ReadAsStringAsync();
      var returnedCompany = JsonConvert.DeserializeObject<Company>(responseBody);
      Assert.Equal("SLB", returnedCompany.Name);
      Assert.NotEmpty(returnedCompany.CompanyId);
    }

    [Fact]
    public async void Should_not_add_new_company_if_existed()
    {
      // given
      var application = new WebApplicationFactory<Program>();
      var httpClient = application.CreateClient();
      await httpClient.DeleteAsync("/companies");
      var company = new Company("SLB");
      await CreateTestSubject(httpClient, company);
      var newCompany = new Company("SLB");
      var companyJson = JsonConvert.SerializeObject(newCompany);
      var postBody = new StringContent(companyJson, Encoding.UTF8, "application/json");
      // when
      var response = await httpClient.PostAsync("/companies", postBody);
      // then
      Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async void Should_get_all_companies_from_system()
    {
      // given
      var httpClient = await InitializeHttpClient();
      var companies = new List<Company>
      {
        new Company("SLB"),
        new Company("TW"),
      };
      foreach (var company in companies)
      {
        await CreateTestSubject(httpClient, company);
      }

      // when
      var response = await httpClient.GetAsync("/companies");
      // then
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      var responseBody = await response.Content.ReadAsStringAsync();
      var returnedCompanies = JsonConvert.DeserializeObject<List<Company>>(responseBody);
      foreach (var company in returnedCompanies)
      {
        Assert.NotEmpty(company.CompanyId);
      }
    }

    [Fact]
    public async void Should_get_company_by_name_from_system()
    {
      // given
      var httpClient = await InitializeHttpClient();
      var companies = new List<Company>
      {
        new Company("SLB"),
        new Company("TW"),
      };
      foreach (var company in companies)
      {
        await CreateTestSubject(httpClient, company);
      }

      // when
      var response = await httpClient.GetAsync($"/companies/{companies[0].CompanyId}");
      // then
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      var responseBody = await response.Content.ReadAsStringAsync();
      var returnedCompany = JsonConvert.DeserializeObject<Company>(responseBody);
      Assert.Equal(companies[0].Name, returnedCompany.Name);
    }

    private static async Task<HttpClient> InitializeHttpClient()
    {
      var application = new WebApplicationFactory<Program>();
      var httpClient = application.CreateClient();
      await httpClient.DeleteAsync("/companies");

      return httpClient;
    }

    private static async Task<string> CreateTestSubject(HttpClient httpClient, Company company)
    {
      var serializedObject = JsonConvert.SerializeObject(company);
      var postBody = new StringContent(serializedObject, Encoding.UTF8, "application/json");
      await httpClient.PostAsync("/companies", postBody);

      return "Test subject created.";
    }
  }
}
