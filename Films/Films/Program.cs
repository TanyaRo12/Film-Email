using MovieRecommendationAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ÊÎÍÔÈÃÓĞÀÖÈß ÑÅĞÂÈÑÎÂ
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ĞÅÃÈÑÒĞÀÖÈß ÍÀØÈÕ ÑÅĞÂÈÑÎÂ
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddLogging();

// ÄÎÁÀÂÜ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ÍÀÑÒĞÎÉÊÀ PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ÈÑÏÎËÜÇÓÉ CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Èçìåíè ïîğò íà 5046
app.Run("http://localhost:5046");