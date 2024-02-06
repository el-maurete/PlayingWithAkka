using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using static Todo.Core.ICommands;
using static Todo.Core.IEvents;

namespace Todo.UI.Pages;

public partial class Todo
{
    [Inject]
    private HttpClient Client { get; set; } = default!;

    private readonly List<TodoItem> _todos = new();
    private string? _newTodo;

    private async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(_newTodo))
        {
            var todo = new Create(Guid.NewGuid().ToString(), _newTodo, _newTodo + ": Created from UI");
            var response = await Client.PostAsJsonAsync("/Api", todo);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<Created>();
            _todos.Add(new TodoItem { Id = result!.Id, Title = result.Title, IsDone = false});
            _newTodo = string.Empty;
        }
    }

    record TodoItem()
    {
        public string? Id { get; init; }
        public string? Title { get; set; }
        public bool IsDone { get; set; }
    }
}