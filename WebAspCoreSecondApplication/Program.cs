using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//app.MapGet("/", () => "Hello World!");
app.Run(async (context) =>
{
    var request = context.Request;
    var response = context.Response;

    response.ContentType = "text/html; charset=utf-8";

    /*
    if (request.Path == "/user")
    {
        var form = request.Form;
        string? name = form["name"];
        string? age = form["age"];

        string[]? langs = form["lang"];

        var strBuilder = new StringBuilder("<div");
        strBuilder.Append($"<p>Name user: {name}, age: {age}</p><ul>");
        foreach (var lang in langs)
            strBuilder.Append($"<li>{lang}</li>");
        strBuilder.Append("</ul></div>");
        
        await response.WriteAsync(strBuilder.ToString());

        //await response.WriteAsync($"Name user: {name}, age: {age}");
    }
    else
        await response.SendFileAsync("html/index.html");
    */
    /*
    if (request.Path == "/old")
        //response?.WriteAsync("Old Page");
        response.Redirect("https://ya.ru");
    else
        response?.WriteAsync("New Page");
    */

    //User user = new() { Name = "Tom", Age = 34 };
    //await response.WriteAsJsonAsync(user);

    if (request.Path == "/user")
    {
        string message = "";

        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new UserConverter());

        var user = await request.ReadFromJsonAsync<User>(jsonOptions);
        if (user != null)
            message = $"Name user: {user.Name}, age: {user.Age}";
        else
            message = "Incorrect data";

        await response.WriteAsJsonAsync(new { text = message });
    }
    else
        await response.SendFileAsync("html/index.html");

});

app.Run();

class User
{
    public string? Name { set; get; }
    public int Age { set; get; }

    public User(string name, int age)
    {
        Name = name;
        Age = age;
    }
}

class UserConverter : JsonConverter<User>
{
    public override User? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var userName = "Undefined";
        var userAge = 0;
        while(reader.Read())
        {
            if(reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();
                switch(propertyName?.ToLower())
                {
                    case "age" when reader.TokenType == JsonTokenType.Number:
                        userAge = reader.GetInt32();
                        break;
                    case "age" when reader.TokenType == JsonTokenType.String:
                        string strValue = reader.GetString();
                        if(Int32.TryParse(strValue, out int value))
                            userAge = value;
                        break;
                    case "name":
                        string? name = reader.GetString();
                        if (name != null)
                            userName = name;
                        break;
                }
            }
        }
        return new User(userName, userAge);
    }

    public override void Write(Utf8JsonWriter writer, User value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", value.Name);
        writer.WriteNumber("age", value.Age);
        writer.WriteEndObject();
    }
}
