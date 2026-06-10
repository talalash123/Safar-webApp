using MongoDB.Driver;
using SafarWebApp.Services; // <-- Added to access your new ML services

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// ==========================================
// SAFAR MONGODB CLIENT INITIALIZATION
// ==========================================
var mongoSettings = builder.Configuration.GetSection("MongoDbSettings");
var mongoClient = new MongoClient(mongoSettings["ConnectionString"]);
var database = mongoClient.GetDatabase(mongoSettings["DatabaseName"]);

// Injecting IMongoDatabase instance across application context
builder.Services.AddSingleton(database);
// ==========================================

// ==========================================
// 🧠 SAFAR AI & ML SERVICES REGISTRATION
// ==========================================
// Registering the services as Singletons so they live across the app
builder.Services.AddSingleton<DynamicPricingService>();
builder.Services.AddSingleton<SeatingArrangementService>();
builder.Services.AddSingleton<ChatbotService>();
// ==========================================

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// ==========================================
// 🤖 SAFAR MINIMAL API FOR CHATBOT
// ==========================================
// This listens for incoming messages from your Javascript UI
app.MapPost("/api/chat", (ChatRequest req, ChatbotService chatbot) =>
{
    var response = chatbot.GetReply(req.Message);
    return Results.Ok(new { reply = response });
});
// ==========================================

// ==========================================
// 🚦 SAFAR DEFAULT ROUTING PIPELINE CONTROL
// ==========================================
// Jab bhi website khulegi, yeh automatic baghair kisi delay ke 
// user ko Customer Portal ke main page par redirect kar dega.
app.MapGet("/", context => {
    context.Response.Redirect("/Customer/Index");
    return System.Threading.Tasks.Task.CompletedTask;
});
// ==========================================

app.MapRazorPages();
app.Run();

// ==========================================
// HELPER CLASSES
// ==========================================
// Required to parse the JSON sent from the Chatbot frontend
public class ChatRequest
{
    public string Message { get; set; }
}