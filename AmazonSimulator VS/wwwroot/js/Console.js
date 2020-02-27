let input = $( '#input' );
let messageID = 0;

// Logs a message in the console
var Log = function ( message, color = '0xffffff' )
{

    let iName = 'no' + messageID;
    let el = '<p id="' + iName + '" ' + "style='display: none; color: " + color + ";'" + '>' + message + '</p>';

    $( '#log' ).prepend( el );

    $( '#' + iName ).toggle( 100 );

    messageID++;

}

input.keypress( function ( event )
{

    if ( event.keyCode == 13 || event.which == 13 ) 
    {

        event.preventDefault();

        // Fetch string typed in input
        let val = input.val();

        // Ignore enter if nothing has been typed in console
        if ( val == '' ) return;

        // Reset console input
        input.val( '' );

        // Split each word in input
        let spl = val.split( ' ' );

        // Command name is the first word
        let cmd = spl[0].toLowerCase();

        // Arguments are the rest, if any 
        let args = spl.slice(1, spl.length);

        // Display entered command in log
        Log( '>> ' + val, '0xffff00' );


        switch ( cmd )
        {
            case 'help':
                consoleHelp( args );
                break;

            case 'receive':
                consoleReceive( args );
                break;

            case 'send':
                consoleSend( args );
                break;

            default:
                Log( 'Unknown command: "' + val + '"', 'red' );
                Log( 'Type "help" to display a list of commands' );
                break;
        }

    }

} );

function consoleHelp( args )
{

    Log( 'Available commands: "send", "Receive"' );

}

function consoleReceive( args )
{

    let amount = 1;

    if ( args.length > 0 && args )
        amount = parseInt( args[0] );

    sendCommand( "ReceiveShipmentCommand", { amount: amount } );

}

function consoleSend( args )
{

    let amount = 1;

    if ( args.length > 0 && args )
        amount = parseInt( args[0] );

    sendCommand( "SendShipmentCommand", { amount: amount }  );

}