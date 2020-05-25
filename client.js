var gameData;

const dataRecived = new Event("dataRecived", {bubbles:true});

function connect()
{
    var ws = new WebSocket('ws://127.0.0.1:2946/BSDataPuller');

    ws.onmessage = function(e) { gameData = JSON.parse(e.data); document.dispatchEvent(dataRecived); }
  
    ws.onclose = function(e)
    {
        validConnection = false;
        console.log('Socket is closed. Reconnect will be attempted in 1 second.', e.reason);
        setTimeout(function()
        {
          connect();
        }, 1000);
    };
}

//Run on load:
connect();