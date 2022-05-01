[assembly: EFCoreRegistration]

namespace SomeNamespace
{
    using Gobie;
    using System.Collections.Generic;

    public enum AuthorState
    { }

    public enum AuthorTrigger
    { }

    [GobieGeneratorName("EFCoreRegistrationAttribute", Namespace = "ConsoleClient")]
    public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
    {
        [GobieGlobalFileTemplate("EFCoreRegistrationGenerator", "EFCoreRegistration")]
        private const string KeyString = @"
            namespace SomeNamespace;

            public sealed static class EFCoreRegistration
            {
                public static void Register(Microsoft.EntityFrameworkCore.ModelBuilder mb)
                {
                    if (mb is null)
                    {
                        throw new ArgumentNullException(nameof(mb));
                    }

                    {{#ChildContent}}
                        {{ ChildContent}}
                    {{/ ChildContent}}
                    {{^ChildContent}}
                        // No generators reference this global generator and are being used.
                    {{/ ChildContent}}
                }
            }
            ";
    }

    public sealed class StatefullLoggedClassGenerator : GobieClassGenerator
    {
        [GobieTemplate]
        private const string StatefullLoggedClass = @"
        /// <summary>
        /// Primary key for the class.
        /// </summary>
        public int Id { get; set; }

        private readonly List<{{ClassName}}StateLog> stateLog = new List<{{ClassName}}StateLog>();

        /// <summary> Readonly Collection of all <see cref=""{{ClassName}}StateLog""/> for the
        /// plate. </summary>
        public IEnumerable<{{ClassName}}StateLog> StateLog => stateLog.AsReadOnly();

        /// <summary>
        /// Current state log entry.
        /// </summary>
        [NotMapped]
        public {{ClassName}}StateLog CurrentStateLogEntry => StateLog.Where(x => x.Current).Single();

        private static partial {{StateEnum}} GetInitialState();

        private readonly StateMachine<{{StateEnum}}, {{TriggerEnum}}> stateMachine;

        /// <summary> Initializes a new instance of the <see cref=""{{ClassName}}""/> class. Ctor
        /// for Entity Framework. </summary>
        private {{ClassName}}()
        {
            // The state machine reads and writes from the local State property.
            stateMachine = GetStateMachine(() => State, s => State = s);
            State = GetInitialState();
        }

        /// <summary>
        /// Builds a new instance of the state machine.
        /// </summary>
        public partial StateMachine<{{StateEnum}}, {{TriggerEnum}}> GetStateMachine(
                Func<{{StateEnum}}> stateAccessor,
                Action<{{StateEnum}}> stateMutator);

        /// <inheritdoc/>
        public bool CanFire({{TriggerEnum}} t)
        {
            if (stateMachine is null)
            {
                throw new Exception(""State machine is missing"");
            }

            return stateMachine.CanFire(t);
        }

        private bool TryFire({{TriggerEnum}} trigger, Action? actionToExecuteIfTriggerFires, User user, string comment)
        {
            bool fireSuceeded = false;
            if (stateMachine is null)
            {
                throw new Exception(""State machine is missing"");
            }

            lock (stateMachine)
            {
                if (stateMachine.CanFire(trigger))
                {
                    stateMachine.Fire(trigger);
                    LogStateChange(trigger, user, comment);
                    fireSuceeded = true;
                }
            }
            if (fireSuceeded)
            {
                actionToExecuteIfTriggerFires?.Invoke();
            }
            return fireSuceeded;
        }

        /// <summary>
        /// Call to log any state change which occurrs.
        /// </summary>
        private void LogStateChange({{TriggerEnum}}? firedTrigger, User user, string comment)
        {
            var currentState = StateLog.Where(x => x.Current).Single();
            currentState.SetNonCurrent();

            stateLog.Add(new {{ClassName}}StateLog(this, user, State, firedTrigger, comment));
        }
        ";

        [GobieFileTemplate("StateLog")]
        private const string KeyString = @"
            using System;

            namespace {{ClassNamespace}};

            public sealed class {{ClassName}}StateLog
            {
                public int Id { get; set; }

                public {{ClassName}} Parent {get; set;}

                public DateTime Timestamp {get; set;}

                public string LogMessage {get; set;}

                public bool Current { get; private set; }

                /// <summary>
                /// Current state after the change.
                /// </summary>
                public {{StateEnum}} State { get; private set; }

                /// <summary>
                /// Trigger that fired.
                /// </summary>
                public {{TriggerEnum}}? FiredTrigger { get; private set; }
            }
            ";

        [GobieGlobalChildTemplate("EFCoreRegistrationGenerator")]
        private const string EfCoreRegistration = @"
            // StatefullLoggedClassGenerator for {{ClassName}} - Map enums to strings and log field.
            mb.Entity<{{ClassName}}>().Property(x => x.State).HasConversion<string>();
            mb.Entity<{{ClassName}}StateLog>().Property(x => x.State).HasConversion<string>();
            mb.Entity<{{ClassName}}StateLog>().Property(x => x.FiredTrigger).HasConversion<string>();
            mb.Entity<{{ClassName}}>()
              .Metadata.FindNavigation(nameof({{ClassName}}.StateLog))?
              .SetPropertyAccessMode(PropertyAccessMode.Field);
";

        [Required]
        public string StateEnum { get; set; }

        [Required]
        public string TriggerEnum { get; set; }
    }

    public sealed class EncapsulatedCollectionGenerator : GobieFieldGenerator
    {
        [GobieTemplate]
        private const string EncapsulatedCollection = @"
            public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : pascal}} => {{FieldName}}.AsReadOnly();

            public void Add{{ FieldName : pascal }}({{FieldGenericType}} s)
            {
                if(s is null)
                {
                    throw new ArgumentNullException(nameof(s));
                }

                {{#CustomValidator}}
                if({{CustomValidator}}(s))
                {
                    {{FieldName}}.Add(s);
                }
                {{/CustomValidator}}

                {{^CustomValidator}}
                    {{FieldName}}.Add(s);
                {{/CustomValidator}}
            }
";

        [GobieGlobalChildTemplate("EFCoreRegistrationGenerator")]
        private const string EfCoreRegistration = @"

            // EncapsulatedCollectionGenerator for {{ClassName}} - Map the encapsulated collection
            mb.Entity<{{ClassName}}>()
              .Metadata.FindNavigation(nameof({{ClassName}}.{{FieldName : Pascal}}))?
              .SetPropertyAccessMode(PropertyAccessMode.Field);

";

        [Required]
        public string CustomValidator { get; set; } = null;
    }

    [StatefullLoggedClass(nameof(AuthorState), nameof(AuthorTrigger))]
    public partial class Author
    {
        [EncapsulatedCollection(null)]
        private readonly List<Publisher> publishers = new();

        [EncapsulatedCollection(nameof(BookValid))]
        private readonly List<Book> books = new();

        private static bool BookValid(Book b) => b.Title.Length > 0;
    }

    public class Publisher
    {
    }

    public class Book
    {
        public string Title { get; set; }
    }
}
