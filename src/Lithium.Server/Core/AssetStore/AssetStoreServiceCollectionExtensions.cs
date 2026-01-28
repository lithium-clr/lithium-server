using Lithium.Server.Core.Resources;

namespace Lithium.Server.Core.AssetStore;

public static class AssetStoreServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAssetStore<T>(Action<AssetStoreOptions> configure)
            where T : AssetResource
        {
            services.Configure(configure);
            services.AddSingleton<IAssetStore, AssetStore<T>>();
            // services.AddSingleton<IAssetStore>(sp => sp.GetRequiredService<AssetStore<T>>());

            return services;
        }

        public IServiceCollection AddAssetStoreRegistry()
        {
            services.AddSingleton<AssetStoreRegistry>();
            return services;
        }
    }
}