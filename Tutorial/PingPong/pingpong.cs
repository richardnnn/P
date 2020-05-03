using Microsoft.Coyote;
using Microsoft.Coyote.Actors;
using Microsoft.Coyote.Runtime;
using Microsoft.Coyote.Specifications;
using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Plang.CoyoteRuntime;
using Plang.CoyoteRuntime.Values;
using Plang.CoyoteRuntime.Exceptions;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 162, 219, 414, 1998
namespace pingpong
{
    public static partial class GlobalFunctions {}
    internal partial class PING : PEvent
    {
        public PING() : base() {}
        public PING (PMachineValue payload): base(payload){ }
        public override IPrtValue Clone() { return new PING();}
    }
    internal partial class PONG : PEvent
    {
        public PONG() : base() {}
        public PONG (IPrtValue payload): base(payload){ }
        public override IPrtValue Clone() { return new PONG();}
    }
    internal partial class SUCCESS : PEvent
    {
        public SUCCESS() : base() {}
        public SUCCESS (IPrtValue payload): base(payload){ }
        public override IPrtValue Clone() { return new SUCCESS();}
    }
    internal partial class Client : PMachine
    {
        private PMachineValue server = null;
        public class ConstructorEvent : PEvent{public ConstructorEvent(IPrtValue val) : base(val) { }}
        
        protected override Event GetConstructorEvent(IPrtValue value) { return new ConstructorEvent((IPrtValue)value); }
        public Client() {
            this.sends.Add(nameof(PING));
            this.sends.Add(nameof(PONG));
            this.sends.Add(nameof(SUCCESS));
            this.sends.Add(nameof(PHalt));
            this.receives.Add(nameof(PING));
            this.receives.Add(nameof(PONG));
            this.receives.Add(nameof(SUCCESS));
            this.receives.Add(nameof(PHalt));
            this.creates.Add(nameof(I_Server));
        }
        
        public void Anon(Event currentMachine_dequeuedEvent)
        {
            Client currentMachine = this;
            PMachineValue TMP_tmp0 = null;
            PEvent TMP_tmp1 = null;
            TMP_tmp0 = (PMachineValue)(currentMachine.CreateInterface<I_Server>( currentMachine));
            server = (PMachineValue)TMP_tmp0;
            TMP_tmp1 = (PEvent)(new SUCCESS(null));
            currentMachine.TryRaiseEvent((Event)TMP_tmp1);
            return;
        }
        public void Anon_1(Event currentMachine_dequeuedEvent)
        {
            Client currentMachine = this;
            PMachineValue TMP_tmp0_1 = null;
            PEvent TMP_tmp1_1 = null;
            PMachineValue TMP_tmp2 = null;
            PEvent TMP_tmp3 = null;
            TMP_tmp0_1 = (PMachineValue)(((PMachineValue)((IPrtValue)server)?.Clone()));
            TMP_tmp1_1 = (PEvent)(new PING(null));
            TMP_tmp2 = (PMachineValue)(currentMachine.self);
            currentMachine.TrySendEvent(TMP_tmp0_1, (Event)TMP_tmp1_1, TMP_tmp2);
            TMP_tmp3 = (PEvent)(new SUCCESS(null));
            currentMachine.TryRaiseEvent((Event)TMP_tmp3);
            return;
        }
        [Start]
        [OnEntry(nameof(InitializeParametersFunction))]
        [OnEventGotoState(typeof(ConstructorEvent), typeof(Init))]
        class __InitState__ : State { }
        
        [OnEntry(nameof(Anon))]
        [OnEventGotoState(typeof(SUCCESS), typeof(SendPing))]
        class Init : State
        {
        }
        [OnEntry(nameof(Anon_1))]
        [OnEventGotoState(typeof(SUCCESS), typeof(WaitPong))]
        class SendPing : State
        {
        }
        [OnEventGotoState(typeof(PONG), typeof(SendPing))]
        class WaitPong : State
        {
        }
    }
    internal partial class Server : PMachine
    {
        public class ConstructorEvent : PEvent{public ConstructorEvent(IPrtValue val) : base(val) { }}
        
        protected override Event GetConstructorEvent(IPrtValue value) { return new ConstructorEvent((IPrtValue)value); }
        public Server() {
            this.sends.Add(nameof(PING));
            this.sends.Add(nameof(PONG));
            this.sends.Add(nameof(SUCCESS));
            this.sends.Add(nameof(PHalt));
            this.receives.Add(nameof(PING));
            this.receives.Add(nameof(PONG));
            this.receives.Add(nameof(SUCCESS));
            this.receives.Add(nameof(PHalt));
        }
        
        public void Anon_2(Event currentMachine_dequeuedEvent)
        {
            Server currentMachine = this;
            PMachineValue payload = (PMachineValue)(gotoPayload ?? ((PEvent)currentMachine_dequeuedEvent).Payload);
            this.gotoPayload = null;
            PMachineValue TMP_tmp0_2 = null;
            PEvent TMP_tmp1_2 = null;
            PEvent TMP_tmp2_1 = null;
            TMP_tmp0_2 = (PMachineValue)(((PMachineValue)((IPrtValue)payload)?.Clone()));
            TMP_tmp1_2 = (PEvent)(new PONG(null));
            currentMachine.TrySendEvent(TMP_tmp0_2, (Event)TMP_tmp1_2);
            TMP_tmp2_1 = (PEvent)(new SUCCESS(null));
            currentMachine.TryRaiseEvent((Event)TMP_tmp2_1);
            return;
        }
        [Start]
        [OnEntry(nameof(InitializeParametersFunction))]
        [OnEventGotoState(typeof(ConstructorEvent), typeof(WaitPing))]
        class __InitState__ : State { }
        
        [OnEventGotoState(typeof(PING), typeof(SendPong))]
        class WaitPing : State
        {
        }
        [OnEntry(nameof(Anon_2))]
        [OnEventGotoState(typeof(SUCCESS), typeof(WaitPing))]
        class SendPong : State
        {
        }
    }
    public class DefaultImpl {
        public static void InitializeLinkMap() {
            PModule.linkMap.Clear();
            PModule.linkMap[nameof(I_Client)] = new Dictionary<string, string>();
            PModule.linkMap[nameof(I_Client)].Add(nameof(I_Server), nameof(I_Server));
            PModule.linkMap[nameof(I_Server)] = new Dictionary<string, string>();
        }
        
        public static void InitializeInterfaceDefMap() {
            PModule.interfaceDefinitionMap.Clear();
            PModule.interfaceDefinitionMap.Add(nameof(I_Client), typeof(Client));
            PModule.interfaceDefinitionMap.Add(nameof(I_Server), typeof(Server));
        }
        
        public static void InitializeMonitorObserves() {
            PModule.monitorObserves.Clear();
        }
        
        public static void InitializeMonitorMap(IActorRuntime runtime) {
            PModule.monitorMap.Clear();
        }
        
        
        [Microsoft.Coyote.SystematicTesting.Test]
        public static void Execute(IActorRuntime runtime) {
            runtime.RegisterLog(new PLogFormatter());
            PModule.runtime = runtime;
            PHelper.InitializeInterfaces();
            PHelper.InitializeEnums();
            InitializeLinkMap();
            InitializeInterfaceDefMap();
            InitializeMonitorMap(runtime);
            InitializeMonitorObserves();
            runtime.CreateActor(typeof(_GodMachine), new _GodMachine.Config(typeof(Main)));
        }
    }
    public class I_Client : PMachineValue {
        public I_Client (ActorId machine, List<string> permissions) : base(machine, permissions) { }
    }
    
    public class I_Server : PMachineValue {
        public I_Server (ActorId machine, List<string> permissions) : base(machine, permissions) { }
    }
    
    public partial class PHelper {
        public static void InitializeInterfaces() {
            PInterfaces.Clear();
            PInterfaces.AddInterface(nameof(I_Client), nameof(PING), nameof(PONG), nameof(SUCCESS), nameof(PHalt));
            PInterfaces.AddInterface(nameof(I_Server), nameof(PING), nameof(PONG), nameof(SUCCESS), nameof(PHalt));
        }
    }
    
    public partial class PHelper {
        public static void InitializeEnums() {
            PrtEnum.Clear();
        }
    }
    
}
#pragma warning restore 162, 219, 414
