// using Lithium.Server.Core;
// using Lithium.TestPlugin.Abstractions;
//
// namespace Lithium.InventoryPlugin;
//
// public sealed class InventoryPlugin : Component, IPlayerEvent
// {
//     public override void OnLoad()
//     {
//         Log.Info("Loading inventory plugin");
//
//         // var player = World.Current.GetPlayer(0);
//         //
//         // if (player is null)
//         // {
//         //     Log.Info("Player not found");
//         //     return;
//         // }
//     }
//
//     public override void OnUnload()
//     {
//         Log.Info("Unloading inventory plugin");
//     }
//
//     public void OnPlayerDied(Client player)
//     {
//         Log.Info("Player died");
//     }
// }