import * as Main from '../js/Main.js';

var DOMConsole = function( )
{
    var messageID = 0;

    function print( message, color = '0xffffff', notifyBrowser = true )
    {

        let iName = 'no' + messageID;
        let el = '<p id="' + iName + '" ' + "style='display: none; color: " + color + ";'" + '>' + message + '</p>';

        $( '#log' ).prepend( el );

        if( notifyBrowser )
            console.log( message );

        $( '#' + iName ).toggle( 100 );

        messageID++;

    }

    function printError( message )
    {
        console.error( message );
        print( message, 'red' );
    }

    function consoleHelp( args )
    {

        print( 'Available commands: "send", "Receive"' );

    }

    function commandReceiveShipment( args )
    {

        let amount = 1;

        if ( args.length > 0 && args )
            amount = parseInt( args[0] );

        print( 'Receiving a shipment with size of ' + amount );
        Main.sendCommand( "ReceiveShipmentCommand", { amount: amount } );

    }

    function commandSendShipment( args )
    {

        let amount = 1;

        if ( args && args.length > 0 )
            amount = parseInt( args[0] );

        print( 'Sending a shipment with max size of ' + amount );
        Main.sendCommand( "SendShipmentCommand", { amount: amount } );

    }

    function onKeyPress ( event )
    {

        if ( event.keyCode == 13 || event.which == 13 )
        {

            event.preventDefault();

            // Fetch string typed in input
            let val = $( '#input' ).val();

            // Ignore enter if nothing has been typed in console
            if ( val == '' ) return;

            // Reset console input
            $( '#input' ).val( '' );

            // Split each word in input
            let spl = val.split( ' ' );

            // Command name is the first word
            let cmd = spl[0].toLowerCase();

            // Arguments are the rest, if any
            let args = spl.slice( 1, spl.length );

            // Display entered command in log
            print( '>> ' + val, '0xffff00' );


            switch ( cmd )
            {
                case 'help':
                    consoleHelp( args );
                    break;

                case 'receive':
                    commandReceiveShipment( args );
                    break;

                case 'send':
                    commandSendShipment( args );
                    break;

                case 'animate':
                    commandAnimate(args);
                    break;

                default:
                    print( 'Unknown command: "' + val + '"', 'red' );
                    print( 'Type "help" to display a list of commands' );
                    break;
            }

        }

    }

    $( document ).ready( () => $( '#input' ).keypress( onKeyPress ));

    return {
        print: print,
        printError: printError
    };
};

export default DOMConsole;
