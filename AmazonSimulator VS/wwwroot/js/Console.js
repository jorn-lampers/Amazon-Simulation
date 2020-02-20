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

        let val = input.val();

        if ( val == '' ) return;

        input.val( '' );

        Log( '>> ' + val, '0xffff00' );

        switch ( val.toLowerCase() )
        {
            case 'help':
                Log( 'Available commands: "send", "Receive"' );
                break;

            case 'receive':
                sendCommand( "ReceiveShipmentCommand", {} );
                break;

            case 'send':
                sendCommand( "SendShipmentCommand", {} );
                break;

            default:
                Log( 'Unknown command: "' + val + '"', 'red' );
                Log( 'Type "help" to display a list of commands' );
                break;
        }

    }

} );