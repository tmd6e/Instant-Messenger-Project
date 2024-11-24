To use the program, you need to have a Windows OS, as this runs using the WinForms GUI. Virtual machines emulating the Windows OS can be used by users on the MacOS, including UTM, VMWare, and Parallels Desktop.
The repository contains a 'InstantMessenger' .exe application that can be ran to test. 
The application contains two distinct features: starting a chatroom and joining a chatroom.
Starting a chatroom will designate the user as a server to host incoming clients if they're provided the server's IP address and port. The IP address can either be retrieved via two different buttons on the interface or manually inputed by the server's user.
Joining a chatroom will designate the user as a client to use a server as a medium of communication to other clients given the server's IP address and port.
If the chatroom server disconnects, all clients will disconnect.

Known Issues: While there is a successful case in which computers can connect successfully to the test computer's server, the test computer was unable to join other computers as a client. Other computers were also not able to join each other. It is unknown whether this issue was because of the code or because of the test network's conditions or topology at the time.
Notes: Currently, the application does not support public IP addresses. This only functions within LANs with private IP addresses.