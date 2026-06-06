using MongoDB.Driver;

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
// ?? SAFAR DEFAULT ROUTING PIPELINE CONTROL
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