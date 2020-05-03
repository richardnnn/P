// PingPong.p
event PING assert 1: machine;
event PONG assert 1;
event SUCCESS;

main machine Client {
    var server: machine;

    start state Init {
        entry {
  	        server = new Server();
	        raise SUCCESS;   	   
        }
        on SUCCESS goto SendPing;
    }

    state SendPing {
        entry {
	          send server, PING, this;
	          raise SUCCESS;
	    }
        on SUCCESS goto WaitPong;
    }

    state WaitPong {
        on PONG goto SendPing;
    }
}

machine Server {
    start state WaitPing {
        on PING goto SendPong;
    }

    state SendPong {
	      entry (payload: machine) {
	          send payload, PONG;
	          raise SUCCESS;		 	  
	      }
        on SUCCESS goto WaitPing;
    }
}