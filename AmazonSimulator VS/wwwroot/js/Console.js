let messageID = 0;

var CONSOLE =
{

    // Logs a message in the console
    Log: function ( message, color = '0xffffff' )
    {

        let iName = 'no' + messageID;
        let el = '<p id="' + iName + '" ' + "style='display: none; color: " + color + ";'" + '>' + message + '</p>';

        $( '#log' ).prepend( el );

        $( '#' + iName ).toggle( 100 );

        messageID++;

    },

    ConsoleHelp: function ( args )
    {

        CONSOLE.Log( 'Available commands: "send", "Receive"' );

    },

    CommandReceiveShipment: function ( args )
    {

        let amount = 1;

        if ( args.length > 0 && args )
            amount = parseInt( args[0] );

        CONSOLE.Log( 'Receiving a shipment with size of ' + amount );
        sendCommand( "ReceiveShipmentCommand", { amount: amount } );

    },

    CommandSendShipment: function ( args )
    {

        let amount = 1;

        if ( args && args.length > 0 )
            amount = parseInt( args[0] );

        CONSOLE.Log( 'Sending a shipment with max size of ' + amount );
        sendCommand( "SendShipmentCommand", { amount: amount } );

    },

    CommandAnimate: function ( args )
    {

      let index = 0;

      if ( args.length > 0 ) index = parseInt( args[0] );

      let reversed = false;

      if ( args.length > 1 && args[1] == "reversed" ) reversed = true;

      CONSOLE.Log('Running animation # ' + index + (reversed ? " reversed" : ""));
      runAnimation( index, reversed );


    }
}

$( document ).ready( function ()
{
    $( '#input' ).keypress( function ( event )
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
            CONSOLE.Log( '>> ' + val, '0xffff00' );


            switch ( cmd )
            {
                case 'help':
                    consoleHelp( args );
                    break;

                case 'receive':
                    CONSOLE.CommandReceiveShipment( args );
                    break;

                case 'send':
                    CONSOLE.CommandSendShipment( args );
                    break;

                case 'animate':
                    CONSOLE.CommandAnimate(args);
                    break;

                default:
                    CONSOLE.Log( 'Unknown command: "' + val + '"', 'red' );
                    CONSOLE.Log( 'Type "help" to display a list of commands' );
                    break;
            }

        }

    } );
} );