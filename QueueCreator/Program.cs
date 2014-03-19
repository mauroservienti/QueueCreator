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
				SetPermissions( q, prms.AddCurrentWindowsUser, prms.CustomUsers );

				if ( prms.CreateSubscriptionsQueue )
				{
					var s = MessageQueue.Create( qn + ".subscriptions", true );
					SetPermissions( s, prms.AddCurrentWindowsUser, prms.CustomUsers );
				}

				if ( prms.CreateTimeoutsQueue )
				{
					var t = MessageQueue.Create( qn + ".timeouts", true );
					SetPermissions( t, prms.AddCurrentWindowsUser, prms.CustomUsers );
				}

				if ( prms.CreateRetriesQueue )
				{
					var r = MessageQueue.Create( qn + ".retries", true );
					SetPermissions( r, prms.AddCurrentWindowsUser, prms.CustomUsers );
				}
			}
		}

		static void SetPermissions( MessageQueue q, Boolean addCurrentWindowsUser, String customUsers )
		{
			q.SetPermissions( "SYSTEM", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow );
			q.SetPermissions( "Administrators", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow );
			q.SetPermissions( WindowsIdentity.GetAnonymous().Name, MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow );
			q.SetPermissions( "Everyone", MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow );
			q.SetPermissions( "Everyone", MessageQueueAccessRights.GetQueueProperties, AccessControlEntryType.Allow );

			if ( addCurrentWindowsUser )
			{
				var identity = WindowsIdentity.GetCurrent();
				var name = identity.Name;
				q.SetPermissions( name, MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow );
			}

			if ( !String.IsNullOrWhiteSpace( customUsers ) ) 
			{
				var users = customUsers.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
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
