using MongoDB.Bson.Serialization.Conventions;

namespace WasteBatteriesRegBackend.Utils.Mongo;

public static class MongoConventions
{
    private static readonly object s_gate = new();
    private static volatile bool s_registered;

    public static void Register()
    {
        if (s_registered)
        {
            return;
        }

        lock (s_gate)
        {
            if (s_registered)
            {
                return;
            }

            var conversions = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };

            ConventionRegistry.Register("CamelCase", conversions, _ => true);
            s_registered = true;
        }
    }
}
