
// если поток видит await то он идет вверх по методам и выполняет процесс который свободен.
async Task<ActionResult> GetUserInfo()
{

    // сначала получаем Айди, потом совершаем два метода одновременно и ждем их завершения
    var userId = await GetUserId();

    var location = GetUserLocation(userId);
    var game = GetUserGame(userId);

    await Task.WhenAll(location, game);

    return Ok(new { userId, location, game });
}

async Task<int> GetUserId()
{
    var userId = await .... // получение Id
}

async Task<string> GetUserLocation(Guid userId)
{
    var location = await .... 
}

async Task<string> GetUserGame(Guid userId)
{
    var game = await ...;
}