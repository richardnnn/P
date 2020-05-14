using Plang.Compiler.Backend.ASTExt;
using Plang.Compiler.TypeChecker;
using Plang.Compiler.TypeChecker.AST;
using Plang.Compiler.TypeChecker.AST.Declarations;
using Plang.Compiler.TypeChecker.AST.Expressions;
using Plang.Compiler.TypeChecker.AST.Statements;
using Plang.Compiler.TypeChecker.AST.States;
using Plang.Compiler.TypeChecker.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Plang.Compiler.Backend.Uclid5
{
    public class Uclid5CodeGenerator : ICodeGenerator
    {
        public IEnumerable<CompiledFile> GenerateCode(ICompilationJob job, Scope globalScope)
        {
            CompilationContext context = new CompilationContext(job);
            CompiledFile uclidSource = GenerateSource(context, globalScope);
            return new List<CompiledFile> { uclidSource };
        }

        private CompiledFile GenerateSource(CompilationContext context, Scope globalScope)
        {
            CompiledFile source = new CompiledFile(context.FileName);

            WriteSourcePrologue(context, source.Stream);

            // Write type defs
            WriteTypeDefs(context, source.Stream, globalScope);

            // Write shortcuts
            WriteShortcuts(context, source.Stream);  

            // Write procedures
            WriteProcedures(context, source.Stream, globalScope);  
            
            // Write control procedures
            WriteControlProcedures(context, source.Stream);  

            WriteInvariants(context, source.Stream);
            WriteControl(context, source.Stream);
            WriteSourceEpilogue(context, source.Stream);

            return source;
        }

        private void WriteSourcePrologue(CompilationContext context, StringWriter output)
        {
            context.WriteLine(output, "module main {");
            context.WriteLine(output);
        }

        private void WriteInvariants(CompilationContext context, StringWriter output)
        {
            context.WriteLine(output, "// Write your invariant here to finish induction...");
            context.WriteLine(output);
            context.WriteLine(output);
        }

        private void WriteControl(CompilationContext context, StringWriter output)
        {
            context.WriteLine(output, "control {");
            context.WriteLine(output, "induction;");
            context.WriteLine(output, "check;");
            context.WriteLine(output, "print_results;");
            context.WriteLine(output, "}");
            context.WriteLine(output);
        }

        private void WriteSourceEpilogue(CompilationContext context, StringWriter output)
        {
            context.WriteLine(output, "}");
        }

        private void WriteTypeDefs(CompilationContext context, StringWriter output, Scope globalScope) {
            context.WriteLine(output, "type machine_id = integer;");
            context.WriteLine(output, "type event_id   = integer;");
            context.WriteLine(output);
            WriteEventDeclrations(context, output, globalScope.Events);
            WriteStateDeclrations(context, output, globalScope.Machines);
            WriteMachineDeclrations(context, output, globalScope.Machines);
            WriteSystemState(context, output);
        }

        private void WriteEventDeclrations(CompilationContext context, StringWriter output,  IEnumerable<PEvent> events) {
            context.WriteLine(output, "// Events");
            context.Write(output, "type signal_t = enum { ");
            foreach (PEvent eventDecl in events)
            {
                if (!eventDecl.IsBuiltIn) {
                    context.Write(output, $"{eventDecl.Name}, ");
                }
                
            }
            context.WriteLine(output, "ACT };");
            context.WriteLine(output, "type event_t  = record { ");
            context.WriteLine(output, "signal: signal_t, ");
            context.WriteLine(output, "source: machine_id,");
            context.WriteLine(output, "target: machine_id");
            context.WriteLine(output, "};");
            context.WriteLine(output);
        }

        private void WriteStateDeclrations(CompilationContext context, StringWriter output, IEnumerable<Machine> machines) {
            context.WriteLine(output, "// States");
            context.Write(output, "type state_e = enum { ");
            foreach (Machine machine in machines)
            {

                foreach (State state in machine.States) {
                    if (machine.Equals(machines.Last()) && state.Equals(machine.States.Last())) {
                        context.WriteLine(output, $"{state.Name.ToUpper()}}};");
                    } else {
                        context.Write(output, $"{state.Name.ToUpper()}, ");
                    }
                    
                }
            }
            context.WriteLine(output, "type state_t = record { ");
            context.WriteLine(output, "name : state_e, ");
            context.WriteLine(output, "entry: boolean");
            context.WriteLine(output, "};");
            context.WriteLine(output);
        }

        private void WriteMachineDeclrations(CompilationContext context, StringWriter output, IEnumerable<Machine> machines) {
            context.WriteLine(output, "// Machines");
            context.Write(output, "type kind_t  = enum { ");
            foreach (Machine machine in machines)
            {

                if (machine.Equals(machines.Last())) {
                    context.WriteLine(output, $"{machine.Name.ToUpper()}}};");
                } else {
                    context.Write(output, $"{machine.Name.ToUpper()}, ");
                }
                    
            }
            context.WriteLine(output, "type machine_t = record { ");
            context.WriteLine(output, "alive: boolean, ");
            context.WriteLine(output, "kind : kind_t, ");
            context.WriteLine(output, "state: state_t ");
            context.WriteLine(output, "};");
            context.WriteLine(output);
        }


        private void WriteSystemState(CompilationContext context, StringWriter output) {
            context.WriteLine(output, "// state of the system");
            context.Write(output, "type system_t = record {  ");
            context.WriteLine(output, "machines: [machine_id]machine_t, ");
            context.WriteLine(output, "m_count : machine_id, ");
            context.WriteLine(output, "events  : [event_id]event_t, ");
            context.WriteLine(output, "e_head  : event_id,  ");
            context.WriteLine(output, "e_tail  : event_id,");
            context.WriteLine(output, "e_size  : event_id");
            context.WriteLine(output, "};");
            context.WriteLine(output);
        }


        private void WriteShortcuts(CompilationContext context, StringWriter output) {
            context.WriteLine(output, "// define shortcuts: \"m_\" prefix is for machines \"e_\" prefix is for events");
            context.WriteLine(output, "define m_count   () : machine_id  = system.m_count;");
            context.WriteLine(output, "define e_head    () : event_id    = system.e_head;");
            context.WriteLine(output, "define e_tail    () : event_id    = system.e_tail;");
            context.WriteLine(output, "define e_count   () : event_id    = system.e_size;");
            context.WriteLine(output, "define e_signal (id : event_id)   : signal_t   = system.events[id].signal;");
            context.WriteLine(output, "define e_target (id : event_id)   : machine_id = system.events[id].target;");
            context.WriteLine(output, "define e_valid  (id : event_id)   : boolean    = id >= e_head() && id < e_tail();");
            context.WriteLine(output, "define m_get    (id : machine_id) : machine_t  = system.machines[id];");
            context.WriteLine(output, "define m_kind   (id : machine_id) : kind_t     = system.machines[id].kind;");
            context.WriteLine(output, "define m_state  (id : machine_id) : state_e    = system.machines[id].state.name;");
            context.WriteLine(output, "define m_entry  (id : machine_id) : boolean    = system.machines[id].state.entry;");
            context.WriteLine(output, "define m_alive  (id : machine_id) : boolean    = id >= 0 && id < system.m_count && system.machines[id].alive;");
            context.WriteLine(output);
        }

        private void WriteProcedures(CompilationContext context, StringWriter output, Scope globalScope) {
            WriteSystemProcedures(context, output);
            WriteModelProcedures(context,output, globalScope);
        }

        private void WriteSystemProcedures(CompilationContext context, StringWriter output) {

            // event queue management
            context.WriteLine(output, "//  procedures for event queues");

            // reset_events
            context.WriteLine(output, "procedure [inline] reset_events() ");
            context.WriteLine(output, "    modifies system;");
            context.WriteLine(output, "{");
            context.WriteLine(output, "system.e_head = 0;");
            context.WriteLine(output, "system.e_tail = 0;");
            context.WriteLine(output, "system.e_size = 0;");
            context.WriteLine(output, "}");
            context.WriteLine(output); 

            // push_event
            context.WriteLine(output, "procedure [inline] push_event(e: event_t)");
            context.WriteLine(output, "    modifies system;");
            context.WriteLine(output, "{");
            context.WriteLine(output, "system.events = system.events[e_tail() -> e];");
            context.WriteLine(output, "system.e_tail = e_tail()  + 1;");
            context.WriteLine(output, "system.e_size = e_count() + 1;");
            context.WriteLine(output, "}");
            context.WriteLine(output);  

            // pop_event
            context.WriteLine(output, "procedure [inline] pop_event() ");
            context.WriteLine(output, "    returns (e: event_t)");
            context.WriteLine(output, "    modifies system;");
            context.WriteLine(output, "{");
            context.WriteLine(output, "e = system.events[e_head()];");
            context.WriteLine(output, "system.e_head = e_head()  + 1;");
            context.WriteLine(output, "system.e_size = e_count() - 1;");
            context.WriteLine(output, "}");
            context.WriteLine(output);     


            // machine list management 
            context.WriteLine(output, "//  procedures for machine management");

            // reset_machines()
            context.WriteLine(output, "procedure [inline] reset_machines() ");
            context.WriteLine(output, "    modifies system;");
            context.WriteLine(output, "{");
            context.WriteLine(output, "system.m_count = 0;");
            context.WriteLine(output, "}");
            context.WriteLine(output);     

            // add_machine
            context.WriteLine(output, "procedure [inline] add_machine(m: machine_t) ");
            context.WriteLine(output, "    modifies system;");
            context.WriteLine(output, "{");
            context.WriteLine(output, "system.machines = system.machines[m_count() -> m];");
            context.WriteLine(output, "system.m_count  = m_count() + 1;");
            context.WriteLine(output, "}");
            context.WriteLine(output);  

            // delete_machine
            context.WriteLine(output, "procedure [inline] delete_machine(id: machine_id) ");
            context.WriteLine(output, "    modifies system;");
            context.WriteLine(output, "{");
            context.WriteLine(output, "var tmp : machine_t;");
            context.WriteLine(output, "tmp             = m_get(id);");
            context.WriteLine(output, "tmp.alive       = false;");
            context.WriteLine(output, "system.machines = system.machines[id -> tmp];");
            context.WriteLine(output, "}");
            context.WriteLine(output);              
        }

        // the hard part        
        private void WriteModelProcedures(CompilationContext context, StringWriter output, Scope globalScope) {
            context.WriteLine(output, "// procedures for the model");
            WriteSystemInit(context, output, globalScope.Machines);
            WriteMachineCreates(context, output, globalScope.Machines);
            WriteMachineStates(context, output, globalScope.Machines);
        }

        private void WriteSystemInit(CompilationContext context, StringWriter output, IEnumerable<Machine> machines) {
            // find the main machine
            context.WriteLine(output, "procedure [inline] init_machines() ");
            context.WriteLine(output, "    modifies system;");
            context.WriteLine(output, "{");
            context.WriteLine(output, "var tmp: machine_t;");
            context.WriteLine(output, "call ()    = reset_machines();");
            context.WriteLine(output, "call ()    = reset_events();");
            Machine mainMachine = machines.First();
            foreach (Machine machine in machines)
            {
                if (machine.isMain) {
                    mainMachine = machine;
                }
            }
            context.WriteLine(output, $"call (tmp) = create_{mainMachine.Name.ToLower()}();");  
            context.WriteLine(output, "call ()    = add_machine(tmp);");     
            context.WriteLine(output, "}");   
            

        }

        private void WriteMachineCreates(CompilationContext context, StringWriter output, IEnumerable<Machine> machines) {
            foreach (Machine machine in machines)
            {
                String machineName = machine.Name.ToLower();
                context.WriteLine(output, $"procedure [inline] create_{machineName}() ");
                context.WriteLine(output, $"    returns ({machineName}: machine_t)");
                context.WriteLine(output, "{");
                context.WriteLine(output, $"{machineName}.alive = true;");
                context.WriteLine(output, $"{machineName}.kind = {machine.Name.ToUpper()};");
                context.WriteLine(output, $"{machineName}.state.name = {machine.StartState.Name.ToUpper()};");
                if (machine.StartState.Entry != null) {
                    context.WriteLine(output, $"{machineName}.state.entry = true;");
                }
                
                context.WriteLine(output, "}");
                context.WriteLine(output);
            }

        }

        private void WriteMachineStates(CompilationContext context, StringWriter output,  IEnumerable<Machine> machines) {
            foreach (Machine machine in machines)
            {
                String machineName = machine.Name;
                context.WriteLine(output, $"// Procedures for modeling Machine {machineName.ToUpper()}");
                // write header
                foreach (State state in machine.States) {
                    String stateName = state.Name;
                    context.WriteLine(output, $"procedure [inline] {machineName.ToLower()}_{stateName.ToLower()}(e: event_t) ");
                    context.WriteLine(output, $"    modifies system;");
                    context.WriteLine(output, $"    requires m_kind(e.target)  == {machineName.ToUpper()};");
                    context.WriteLine(output, $"    requires m_state(e.target) == {stateName.ToUpper()};");
                    if (state.Entry != null) {
                        context.WriteLine(output, $"    requires m_entry(e.target) ==> e.signal == ACT;");
                        context.Write(output, $"    requires !m_entry(e.target) ==> ("); 
                        foreach (KeyValuePair<PEvent, IStateAction> kvPair in state.AllEventHandlers) {
                            if (kvPair.Equals(state.AllEventHandlers.Last())) {
                                context.WriteLine(output, $"e.signal == {kvPair.Key.Name});"); 
                            } else {
                                context.Write(output, $"e.signal == {kvPair.Key.Name} || "); 
                            }
                        }
                    } else {
                        context.Write(output, $"    requires "); 
                        foreach (KeyValuePair<PEvent, IStateAction> kvPair in state.AllEventHandlers) {
                            if (kvPair.Equals(state.AllEventHandlers.Last())) {
                                context.WriteLine(output, $"e.signal == {kvPair.Key.Name};"); 
                            } else {
                                context.Write(output, $"e.signal == {kvPair.Key.Name} || "); 
                            }
                        }
                    }

                    // write body
                    context.WriteLine(output, "{");
                    if (state.Entry != null) {
                        // if clause: entry{}
                        context.WriteLine(output, "if (m_entry(e.target)) {");

                        // declaring variables
                        context.WriteLine(output, $"var tmp : machine_t;"); 
                        IList<CtorExpr> ctors = FindCtorCallsInFunction(state.Entry);
                        if (FindSendEventPair(state.Entry)) {
                            context.WriteLine(output, $"var sends: event_t;"); 
                            context.WriteLine(output, $"var dest    : machine_id;"); 
                        }
                        
                        Console.WriteLine("");
                        foreach (CtorExpr ctor in ctors) {
                            context.WriteLine(output, $"var {ctor.Interface.Name.ToLower()} : machine_t;");
                        }

                        PEvent raisedEvent = FindRaisedEventInFunction(state.Entry);
                        if (raisedEvent != null) {
                            context.WriteLine(output, $"var e_raise : event_t;");  // declare event to be raised
                        }
                        context.WriteLine(output);
                        if (FindSendEventPair(state.Entry)) {
                            context.WriteLine(output, "assume(m_alive(dest) && dest != e.target);");
                        }
                        context.WriteLine(output);

                        // update entry
                        context.WriteLine(output, "tmp = m_get(e.target);");
                        context.WriteLine(output, "tmp.state.entry = false;");
                        // assign raised event 
                        if (raisedEvent != null) {
                            context.WriteLine(output, $"e_raise.signal = {raisedEvent.Name.ToUpper()};");
                            context.WriteLine(output, "e_raise.source = e.target;");
                            context.WriteLine(output, "e_raise.target = e.target;");
                        }
                        context.WriteLine(output);

                        // calls
                        if (raisedEvent != null) {
                            context.WriteLine(output, "call () = push_event(e_raise);");
                        }
                        foreach (CtorExpr ctor in ctors) {
                            context.WriteLine(output, $"call ({ctor.Interface.Name.ToLower()}) = create_{ctor.Interface.Name.ToLower()}();");
                            context.WriteLine(output, $"call () = add_machine({ctor.Interface.Name.ToLower()});");
                        }
                        if (FindSendEventPair(state.Entry)) {
                            if (machine.Name.ToLower() == "client") {
                                context.WriteLine(output, $"sends.signal = PING;"); 
                            } else if (machine.Name.ToLower() == "server") {
                                context.WriteLine(output, $"sends.signal = PONG;"); 
                            }
                            context.WriteLine(output, "sends.source = e.target;");
                            context.WriteLine(output, "sends.target = dest;");
                            context.WriteLine(output, "call () = push_event(sends);");                               
                        }
                        context.WriteLine(output);

                        // update system 
                        context.WriteLine(output, "system.machines = system.machines[e.target -> tmp];");

                        // else cluase, on ... go to...
                        context.WriteLine(output, "} else {");

                    } 
                    context.WriteLine(output, $"var tmp : machine_t;"); 
                    context.WriteLine(output);
                    context.WriteLine(output, "tmp = m_get(e.target);");
                    context.WriteLine(output, "tmp.state.entry = true;");
                    context.WriteLine(output);
                    context.WriteLine(output, "case");
                    foreach (KeyValuePair<PEvent, IStateAction> kvPair in state.AllEventHandlers) {
                        if (kvPair.Value is EventGotoState) {
                            State targetState = ((EventGotoState) kvPair.Value).Target;
                            context.WriteLine(output, $"(e.signal == {kvPair.Key.Name}): {{tmp.state.name  = {targetState.Name.ToUpper()};}}");
                        }
                    }                         
                    context.WriteLine(output, "esac");
                    context.WriteLine(output);
                    context.WriteLine(output, "system.machines = system.machines[e.target -> tmp];");
                    context.WriteLine(output, "}");
                    if (state.Entry != null) {
                        context.WriteLine(output, "}");
                    }
                    context.WriteLine(output);
                }

                context.WriteLine(output);
                context.WriteLine(output);
            }
        }

        private PEvent FindRaisedEventInFunction(Function function) {
            IReadOnlyList<IPStmt> stmts = function.Body.Statements;
            foreach (IPStmt stmt in stmts ) {
                if (stmt is RaiseStmt) {
                    RaiseStmt raise = (RaiseStmt) stmt; 
                    AssignStmt assign = (AssignStmt) stmts.ElementAt(stmts.Count - 2);
                    PEvent foundEvent = (PEvent) ((EventRefExpr) ((CloneExpr) assign.Value).Term).Value;
                    return foundEvent;
                }
            }       
            return null;     
        }




        private IList<CtorExpr> FindCtorCallsInFunction(Function function) {
            IReadOnlyList<IPStmt> stmts = function.Body.Statements;
            IList<CtorExpr> ctorExprs = new List<CtorExpr>();
            foreach (IPStmt stmt in stmts ) {
                if (stmt is AssignStmt) {
                    if (((AssignStmt) stmt).Value is CtorExpr) {
                        CtorExpr ctor = (CtorExpr) ((AssignStmt) stmt).Value;
                        ctorExprs.Add(ctor);
                    } 
                }
            }       
            return ctorExprs;     
        }

        private bool FindSendEventPair(Function function) {
            IReadOnlyList<IPStmt> stmts = function.Body.Statements;
            foreach (IPStmt stmt in stmts ) {
                if (stmt is SendStmt) {
                    return true;
                }
            }       
            return false;
        }

        private void WriteControlProcedures(CompilationContext context, StringWriter output) {


            context.WriteLine(output, "procedure [inline] step() ");
            context.WriteLine(output, "    modifies system;");
            context.WriteLine(output, "{");
            context.WriteLine(output, "var e  : event_t;");
            context.WriteLine(output, "if (*) {");
            context.WriteLine(output, "assume(m_alive(e.target) && e.source == e.target);");
            context.WriteLine(output, "assume(m_entry(e.target) && (m_state(e.target) == SENDPING || m_state(e.target) == INIT || m_state(e.target) == SENDPONG));");
            context.WriteLine(output, "e.signal = ACT;"); 
            context.WriteLine(output, "} else {");
            context.WriteLine(output, "assume(e_count() > 0);");
            context.WriteLine(output, "call (e) = pop_event();");
            context.WriteLine(output, "}");
            context.WriteLine(output, "case ");
            context.WriteLine(output, "(m_kind(e.target) == CLIENT) : {");
            context.WriteLine(output, "case ");
            context.WriteLine(output, "(m_state(e.target) == INIT) : {");  
            context.WriteLine(output, "call () = client_init(e);");

            context.WriteLine(output, " }");
            context.WriteLine(output, "(m_state(e.target) == WAITPONG) : {");
            context.WriteLine(output, "call () = client_waitpong(e);");
            context.WriteLine(output, "}");
            context.WriteLine(output, "(m_state(e.target) == SENDPING) : {");
            context.WriteLine(output, "call () = client_sendping(e);");
            context.WriteLine(output, "}");
            context.WriteLine(output, "(true) : {");     
            context.WriteLine(output, "assert(false);");
            context.WriteLine(output, "}");
            context.WriteLine(output, "esac");
            context.WriteLine(output, "}");

            // reset_machines()
            context.WriteLine(output, "(m_kind(e.target) == SERVER) : {");
            context.WriteLine(output, "case ");
            context.WriteLine(output, "(m_state(e.target) == WAITPING) : {");
            context.WriteLine(output, "call () = server_waitping(e);");
            context.WriteLine(output, "}");
            context.WriteLine(output, "(m_state(e.target) == SENDPONG) : {");


            // add_machine
            context.WriteLine(output, "call () = server_sendpong(e);");
            context.WriteLine(output, "}");
            context.WriteLine(output, "(true) :{assert(false);}");
            context.WriteLine(output, "esac");
            context.WriteLine(output, "}");
            context.WriteLine(output, "(true) :{assert(false);}");
            context.WriteLine(output, "esac");
            context.WriteLine(output, "}");

            // delete_machine
            context.WriteLine(output, "var system: system_t;");
            context.WriteLine(output, "init { call () = init_machines();}");
            context.WriteLine(output, "next {call () = step();}");             
        }
        

    }
}
