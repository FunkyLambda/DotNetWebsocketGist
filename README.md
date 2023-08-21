This is a terminal app that connects to the Bitmex websocket API, subscribing to the trade and orderbook topics.
It simply prints every message received.

To run this terminal app, simply press the Play button in Visual Studio for Mac.
The project is setup to open in an external Terminal window.

To capture CPU% usage, you can run the following command in another external Terminal window:
`file="~/Documents/cpu_usage.txt"; echo "\n\n" >> $file; date -u >> $file; echo "-------" >> $file; while sleep 1; do ps -p $(ps aux | grep -v grep | grep -i dotnet | awk '{print $2;}') -o %cpu= >> $file; done`

This will print the CPU% usage to a file called `cpu_usage.txt` in the `Documents` directory.
Feel free to set the filename/path to something else.

It would be good to configure the project to execute the above command on run.
