using System.Text.Json;
using System.Text.Json.Nodes;
using LeagueProxyLib;

var leagueProxy = new LeagueProxy();

leagueProxy.Events.OnProcessConfigPublic += (string content) =>
{
    var configObject = JsonSerializer.Deserialize<JsonNode>(content);

    // disable vanguard
    if (configObject?["anticheat.vanguard.enabled"] is not null)
    {
        Console.WriteLine("patched anticheat.vanguard.enabled");
        configObject["anticheat.vanguard.enabled"] = false;
    }

    if (configObject?["lol.client_settings.vanguard.enabled"] is not null)
    {
        Console.WriteLine("patched lol.client_settings.vanguard.enabled");
        configObject["lol.client_settings.vanguard.enabled"] = false;
    }

    var lol = configObject?["keystone.products.league_of_legends.patchlines.live"];
    if (lol is not null)
    {
        var configs = lol["platforms"]?["win"]?["configurations"]?.AsArray();
        if (configs is not null)
        {
            for (var i = 0; i < configs.Count; ++i)
            {
                var dependencies = configs[i]!["dependencies"]!.AsArray();

                var vanguard = dependencies.FirstOrDefault(x => x!["id"]!.GetValue<string>() == "vanguard");
                if (vanguard is not null)
                {
                    Console.WriteLine("removing vanguard dependency for league");
                    dependencies.Remove(vanguard);
                }
            }
        }
    }

    var val = configObject?["keystone.products.valorant.patchlines.live"];
    if (val is not null)
    {
        var configs = val["platforms"]?["win"]?["configurations"]?.AsArray();
        if (configs is not null)
        {
            for (var i = 0; i < configs.Count; ++i)
            {
                var dependencies = configs[i]!["dependencies"]!.AsArray();

                var vanguard = dependencies.FirstOrDefault(x => x!["id"]!.GetValue<string>() == "vanguard");
                if (vanguard is not null)
                {
                    Console.WriteLine("removing vanguard dependency for valorant");
                    dependencies.Remove(vanguard);
                }
            }
        }
    }


    return JsonSerializer.Serialize(configObject);
};

var process = leagueProxy.StartAndLaunchRCS(args);
if (process is null)
{
    Console.WriteLine("Failed to create RCS process!");
    leagueProxy.Stop();
    return;
}

await process.WaitForExitAsync();

// Gracefully exit.
leagueProxy.Stop();
