using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Topics.Radical.Helpers;

namespace QueueCreator
{
	class Program
	{
		static void Main( string[] args )
		{
			var cmdLine = CommandLine.GetCurrent();
			var prms = cmdLine.As<Parameters>();

			var qn = @".\private$\" + prms.QueueName;

			if ( !MessageQueue.Exists( qn ) )
			{
				var q = MessageQueue.Create( qn, true );
				SetPermissions( q, prms );

				if ( prms.CreateSubscriptionsQueue )
				{
					var s = MessageQueue.Create( qn + ".subscriptions", true );
					SetPermissions( s, prms );
				}

				if ( prms.CreateTimeoutsQueue )
				{
					var t = MessageQueue.Create( qn + ".timeouts", true );
					SetPermissions( t, prms );
				}

				if ( prms.CreateRetriesQueue )
				{
					var r = MessageQueue.Create( qn + ".retries", true );
					SetPermissions( r, prms );
				}
			}
		}

		static string GetLocalizedName(WellKnownSidType sidType) 
		{
			var sid = new SecurityIdentifier( WellKnownSidType.WorldSid, null );
			var reference = sid.Translate( typeof( System.Security.Principal.NTAccount ) );
			var name = reference.ToString();

			return name;
		}

		static void SetPermissions( MessageQueue q, Parameters prms )
		{
			var administrators = GetLocalizedName( WellKnownSidType.BuiltinAdministratorsSid );
			var everyone = GetLocalizedName( WellKnownSidType.WorldSid );
			var anonymous = WindowsIdentity.GetAnonymous().Name;

			q.SetPermissions( "SYSTEM", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow );
			q.SetPermissions( administrators, MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow );
			q.SetPermissions( anonymous, MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow );
			q.SetPermissions( everyone, MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow );
			q.SetPermissions( everyone, MessageQueueAccessRights.GetQueueProperties, AccessControlEntryType.Allow );

			if ( prms.AddCurrentWindowsUser )
			{
				var identity = WindowsIdentity.GetCurrent();
				var name = identity.Name;
				q.SetPermissions( name, MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow );
			}

			if ( !String.IsNullOrWhiteSpace( prms.CustomUsers ) ) 
			{
				var users = prms.CustomUsers.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
					.Select( u => u.Trim() );

				foreach ( var user in users ) 
				{
					q.SetPermissions( user, MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow );
				}
			}
		}
	}

	class Parameters
	{
		public Parameters()
		{
			this.CreateRetriesQueue = true;
			this.CreateSubscriptionsQueue = true;
			this.CreateTimeoutsQueue = true;
		}

		[CommandLineArgument( "Name", Aliases = new[] { "n" } )]
		public String QueueName { get; set; }

		[CommandLineArgument( "CreateRetriesQueue", IsRequired = false, Aliases = new[] { "rq" } )]
		public Boolean CreateRetriesQueue { get; set; }

		[CommandLineArgument( "CreateSubscriptionsQueue", IsRequired = false, Aliases = new[] { "sq" } )]
		public Boolean CreateSubscriptionsQueue { get; set; }

		[CommandLineArgument( "CreateTimeoutsQueue", IsRequired = false, Aliases = new[] { "tq" } )]
		public Boolean CreateTimeoutsQueue { get; set; }

		[CommandLineArgument( "AddCurrentWindowsUser", IsRequired = false, Aliases = new[] { "acwu" } )]
		public Boolean AddCurrentWindowsUser { get; set; }

		[CommandLineArgument( "CustomUsers", IsRequired = false, Aliases = new[] { "users" } )]
		public String CustomUsers { get; set; }
	}
}
