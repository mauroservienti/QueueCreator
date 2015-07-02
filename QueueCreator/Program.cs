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

            if( !String.IsNullOrWhiteSpace( prms.QueueName ) )
            {
                var qn = @".\private$\" + prms.QueueName;

                if( !MessageQueue.Exists( qn ) )
                {
                    var q = MessageQueue.Create( qn, true );
                    SetPermissions( q, prms );

                    if( prms.CreateSubscriptionsQueue )
                    {
                        var s = MessageQueue.Create( qn + ".subscriptions", true );
                        SetPermissions( s, prms );
                    }

                    if( prms.CreateTimeoutsQueue )
                    {
                        //v3.x
                        var name = ".timeouts";

                        var t = MessageQueue.Create( qn + name, true );
                        SetPermissions( t, prms );

                        if( prms.CompatibilityLevel.StartsWith( "v4", StringComparison.OrdinalIgnoreCase ) )
                        {
                            name = ".TimeoutsDispatcher";

                            var td = MessageQueue.Create( qn + name, true );
                            SetPermissions( td, prms );
                        }
                    }

                    if( prms.CreateRetriesQueue )
                    {
                        var r = MessageQueue.Create( qn + ".retries", true );
                        SetPermissions( r, prms );
                    }
                }
            }

            if( prms.CreateErrorQueue )
            {
                var r = MessageQueue.Create( @".\private$\error", true );
                SetPermissions( r, prms );
            }

            if( prms.CreateAuditQueue )
            {
                var r = MessageQueue.Create( @".\private$\audit", true );
                SetPermissions( r, prms );
            }

            if( prms.CreateErrorLogQueue )
            {
                var r = MessageQueue.Create( @".\private$\error.log", true );
                SetPermissions( r, prms );
            }

            if( prms.CreateAuditLogQueue )
            {
                var r = MessageQueue.Create( @".\private$\audit.log", true );
                SetPermissions( r, prms );
            }
        }

        static string GetLocalizedName( WellKnownSidType sidType )
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
            var anonymous = GetLocalizedName( WellKnownSidType.AnonymousSid );

            q.SetPermissions( "SYSTEM", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow );
            q.SetPermissions( administrators, MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow );
            q.SetPermissions( anonymous, MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow );
            q.SetPermissions( everyone, MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow );
            q.SetPermissions( everyone, MessageQueueAccessRights.GetQueueProperties, AccessControlEntryType.Allow );

            if( prms.AddCurrentWindowsUser )
            {
                var identity = WindowsIdentity.GetCurrent();
                var name = identity.Name;
                q.SetPermissions( name, MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow );
            }

            if( !String.IsNullOrWhiteSpace( prms.CustomUsers ) )
            {
                var users = prms.CustomUsers.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( u => u.Trim() );

                foreach( var user in users )
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
            this.CompatibilityLevel = "v4";
        }

        [CommandLineArgument( "Name", IsRequired = false, Aliases = new[] { "n" } )]
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

        [CommandLineArgument( "CompatibilityLevel", IsRequired = false, Aliases = new[] { "cl" } )]
        public String CompatibilityLevel { get; set; }

        [CommandLineArgument( "CreateErrorQueue", IsRequired = false, Aliases = new[] { "error" } )]
        public Boolean CreateErrorQueue { get; set; }

        [CommandLineArgument( "CreateAudiQueue", IsRequired = false, Aliases = new[] { "audit" } )]
        public Boolean CreateAuditQueue { get; set; }

        [CommandLineArgument( "CreateErrorLogQueue", IsRequired = false, Aliases = new[] { "error.log" } )]
        public Boolean CreateErrorLogQueue { get; set; }

        [CommandLineArgument( "CreateAudiLogQueue", IsRequired = false, Aliases = new[] { "audit.log" } )]
        public Boolean CreateAuditLogQueue { get; set; }
    }
}
