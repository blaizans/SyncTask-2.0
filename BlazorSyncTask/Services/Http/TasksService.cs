using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Dtos;
using Shared.Models;

namespace BlazorSyncTask.Services.Http;


public class TasksService: ITasksService
{
    private readonly HttpClient client;

    public TasksService(HttpClient client)
    {
        this.client = client;
    }


    public async Task CreateTask(CreateTaskDto createTaskDto)
    {
        CreateTaskDto createTask = new()
        {
            userId = createTaskDto.userId,
            startDate = createTaskDto.startDate,
            endDate = createTaskDto.endDate,
            name = createTaskDto.name,
            description = createTaskDto.description,
        };

        string taskAsJson = JsonSerializer.Serialize(createTaskDto);
     
        StringContent content = new(taskAsJson, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtAuthService.Jwt);
        HttpResponseMessage response = await client.PostAsync("http://localhost:8080/api/tasks/", content);
        string responseContent = await response.Content.ReadAsStringAsync();
     
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseContent);
        }
      
    }

    public async Task<List<TaskDTO>> GetAllTasksByUserId(int userId)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtAuthService.Jwt);
        HttpResponseMessage response = await client.GetAsync("http://localhost:8080/api/tasks/byUser/"+userId);
        string responseContent = await response.Content.ReadAsStringAsync();
    
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseContent);
        }

        List<TaskDTO> pendingUsers = JsonSerializer.Deserialize<List<TaskDTO>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException();
        return pendingUsers;
    }
}