namespace ExitGames.Client.Photon.LoadBalancing
{
	public class WebFlags
	{
		public static readonly WebFlags Default = new WebFlags(0);

		public byte WebhookFlags;

		public const byte HttpForwardConst = 1;

		public const byte SendAuthCookieConst = 2;

		public const byte SendSyncConst = 4;

		public const byte SendStateConst = 8;

		public bool HttpForward
		{
			get
			{
				return (WebhookFlags & 1) != 0;
			}
			set
			{
				WebhookFlags |= 1;
			}
		}

		public bool SendAuthCookie
		{
			get
			{
				return (WebhookFlags & 2) != 0;
			}
			set
			{
				WebhookFlags |= 2;
			}
		}

		public bool SendSync
		{
			get
			{
				return (WebhookFlags & 4) != 0;
			}
			set
			{
				WebhookFlags |= 4;
			}
		}

		public bool SendState
		{
			get
			{
				return (WebhookFlags & 8) != 0;
			}
			set
			{
				WebhookFlags |= 8;
			}
		}

		public WebFlags(byte webhookFlags)
		{
			WebhookFlags = webhookFlags;
		}
	}
}
