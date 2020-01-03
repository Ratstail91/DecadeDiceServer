using System;
using Steamworks;

namespace DecadeDice {
	class DecadeDice {
		//steam-specific connection data
		const uint HOST = 0;
		const ushort AUTHENTICATION_PORT = 8766;
		const ushort GAME_PORT = 27015;
		const ushort SPECTATE_PORT = 27017;
		const ushort UPDATER_PORT = 27016;
		const string SERVER_VERSION = "0.0.0.1";

		//steam callback handlers
		static Callback<SteamServersConnected_t> steamServersConnected;
		static Callback<SteamServerConnectFailure_t> steamServersConnectFailure;
		static Callback<SteamServersDisconnected_t> steamServersDisconnected;
		static Callback<GSPolicyResponse_t> policyResponse;

		static Callback<ValidateAuthTicketResponse_t> gsAuthTicketResponse;
		static Callback<P2PSessionRequest_t> p2pSessionRequest;
		static Callback<P2PSessionConnectFail_t> p2pSessionConnectFail;

		//internal members
		static bool initialized;
		static bool running;
		static bool connectedToSteam;

		static string serverName = "DecadeDiceServer";
		static int maxPlayerCount = 8;

		static void Main(string[] args) {
			if (Initialize() != 0) {
				return;
			}

			Run();

			End();
		}

		//lifetime functions
		static int Initialize() {
			//callbacks
			steamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(OnSteamServersConnected);
			steamServersConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(OnSteamServersConnectFailure);
			steamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(OnSteamServersDisconnected);
			policyResponse = Callback<GSPolicyResponse_t>.CreateGameServer(OnPolicyResponse);

			gsAuthTicketResponse = Callback<ValidateAuthTicketResponse_t>.CreateGameServer(OnValidateAuthTicketResponse);
			p2pSessionRequest = Callback<P2PSessionRequest_t>.CreateGameServer(OnP2PSessionRequest);
			p2pSessionConnectFail = Callback<P2PSessionConnectFail_t>.CreateGameServer(OnP2PSessionConnectFail);

			//internal members
			running = false;
			connectedToSteam = false;

			//initialize the game server
			initialized = GameServer.Init(
				HOST,
				AUTHENTICATION_PORT,
				GAME_PORT,
				UPDATER_PORT,
				EServerMode.eServerModeAuthenticationAndSecure,
				SERVER_VERSION
			);

			if (!initialized) {
				Console.WriteLine("Failed to initialize the game server");
				return -1;
			}

			//set any parameters
			SteamGameServer.SetModDir("decadedice");

			SteamGameServer.SetProduct("DecadeDiceServer");
			SteamGameServer.SetGameDescription("Decade Dice Server");

			SteamGameServer.SetSpectatorPort(SPECTATE_PORT);
			SteamGameServer.SetSpectatorServerName("DecadeDiceSpectate");

			//log on
			SteamGameServer.LogOnAnonymous();
			SteamGameServer.EnableHeartbeats(true);

			//debugging
			Console.WriteLine("Server Initialized");

			return 0;
		}

		static int Run() {
			if (!initialized) {
				return -1;
			}

			running = true;
			while(running) {
				GameServer.RunCallbacks();

				if (connectedToSteam) {
					SendUpdatedServerDetailsToSteam();
				}
			}

			return 0;
		}

		static void End() {
			if (!initialized) {
				return;
			}

			//cleanup
			SteamGameServer.EnableHeartbeats(false);
			steamServersConnected.Dispose();
			SteamGameServer.LogOff();
			GameServer.Shutdown();

			initialized = false;

			Console.WriteLine("Shutdown");
		}

		//callbacks
		static void OnSteamServersConnected(SteamServersConnected_t loginSuccess) {
			Console.WriteLine("DecadeDiceServer connect to steam successfully");

			connectedToSteam = true;

			SendUpdatedServerDetailsToSteam();
		}

		static void OnSteamServersConnectFailure(SteamServerConnectFailure_t connectFailure) {
			connectedToSteam = false;
			Console.WriteLine("DecadeDiceServer failed to connect to steam");
		}

		static void OnSteamServersDisconnected(SteamServersDisconnected_t loggedOut) {
			connectedToSteam = false;
			Console.WriteLine("DecadeDiceServer was logged out of steam");
		}

		static void OnPolicyResponse(GSPolicyResponse_t policyResponse) {
			if (SteamGameServer.BSecure()) {
				Console.WriteLine("DecadeDiceServer is VAC secure");
			} else {
				Console.WriteLine("DecadeDiceServer is not VAC secure!");
			}

			Console.WriteLine("Game server SteamID: " + SteamGameServer.GetSteamID().ToString());
		}

		static void OnValidateAuthTicketResponse(ValidateAuthTicketResponse_t response) {
			Console.WriteLine("OnValidateAuthTicketResponse called steamID: " + response.m_SteamID);

			if (response.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseOK) {
				//TODO: this was commented out in the example
			} else {
				//TODO: this was commented out in the example
			}
		}

		static void OnP2PSessionRequest(P2PSessionRequest_t callback) {
			Console.WriteLine("OnP2PSessionRequest called steamIDRemote: " + callback.m_steamIDRemote);

			SteamGameServerNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote);
		}

		static void OnP2PSessionConnectFail(P2PSessionConnectFail_t callback) {
			Console.WriteLine("OnP2PSessionConnectFail called steamIDRemote: " + callback.m_steamIDRemote);

			//TODO: socket closed, kick the user associated with it.
		}

		//utility functions
		static void SendUpdatedServerDetailsToSteam() {
			SteamGameServer.SetServerName(serverName);
			SteamGameServer.SetMaxPlayerCount(maxPlayerCount);
			SteamGameServer.SetPasswordProtected(false);
		}
	}
}