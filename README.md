## External-Mitigations
In this PoC analysis, we will overview the possible mitigations against external pixel search and input based scripts such as [SpaceSharp](https://lol-script.com/) which are currently out in the market.

 - [Technologies and Techniques Applied in external scripts](#technologies-and-techniques-applied-in-external-scripts)  
    - [Windows GDI and BitBlt Screen Capturing]()
    - [Input simulation with WinAPI]()
    - [Access of game data through League of Legends LiveClientData API]()
 - [Mitigations](mitigations)
    - [Screen Capture/Screenshot Prevention]()
    - [Simulated Input Blocking]()
    - [Limiting LiveClientData API Access]()
 - [Heuristic Measures](heuristic-measures)

## Technologies and Techniques applied in external scripts

#### Windows GDI and BitBlt Screen Capturing 
 - gdi32.dll - BitBlt
 - IDXGIOutputDuplication(Desktop Duplication API) - AcquireNextFrame

#### Input simulation with WinAPI

 - SendMessage
 - mouse_event
 - SetCursorPos
 - SendInput
 - keybd_event

#### Access of game data through League of Legends LiveClientData API

 - https://127.0.0.1:2999

## Mitigations

 ### **Screen Capture/Screenshot Prevention**
From the observation taken, external scripts performs pixel search through the captured/copied graphic buffers from the screen. The most common technique used to capture, is the WinAPI gdi32.dll's **BitBlt** function, followed by the Desktop Duplication API's **AcquireNextFrame**.

One measure against screen capturing outlined in the PoC, is to simply switch display context by setting the **DisplayAffinity** to "monitor only" with user32.dll's [**SetWindowDisplayAffinity**](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowdisplayaffinity) function.

What it initially does is, it will store the display context(affinity) in kernel land where the handle to the window is associated at.

In action, after setting the display affinity, all screen captures are displayed in black plain screen.

**Remarks**
One remark to be taken from this measure is that, setting the display affinity is very fragile.
Why? Not only it limits the display to the monitor but also prevents **PrintScreen** key to work, which may raise issues to the normal users where they're simply taking screenshots with no malicious intent.
To safely handle that issue, **SetWindowDisplayAffinity** is scheduled to be called in occurrence of input simulations. 

 ### **Simulated Input Blocking**
Input simulations are used in conjunction with pixel search operation, where the script will fire needed inputs in order to gain advantage such as automatic combo pressing (aka Macro) and Orbwalking(aka Kiting).

To effectively take measure against the input simulations, Low-Level Input (Keyboard & Mouse) hooks were used along with **WndProc WindowsMessage** filtration and **GetRawInputData** verification.

when **mouse_event** or **SendInput** is called, Windows sets a **LLMHF_INJECTED** flag and a **LLKMF_INJECTED** flag respectively. Those are the flags that are monitored to block input calls. 
Along with monitoring of the flags, input devices' handle is monitored with **GetRawInputData** function inside **WndProc** filtration.
When an input is simulated, input devices's handle doesn't gets set and remains on a value **0**, where the value **0** is monitored inside the **WndProc** hook.

**Remarks**
While it is easier to take measures against the external input simulations, things will get quite tricky once **HID** and **Mouse Drivers** comes in from the kernel land. One idea to subtly monitor the input simulations from drivers, is to check all the loaded device objects and compare it against any **HID** or **mouse drivers** installed that are not part of the physical device. Although vulnerability follows as where one can simply patch out the device handle table and such forth.

On a different note, **low-level input hooks** can be removed and be replaced, where one can simply setup a hook before-hand to filter out our hook being installed. One possible measure is to, check if there's any existing **Windows Hook** installed and simply replace them out; as well as continuously monitor for any additional hook replacement that may happen from external. 
 
 ### **Limiting LiveClientData API Access**
External scripts which are pixel search based are bounded to staying away from reading memory, where they heavily rely on the locally hosted **LiveClientData API**.

There are countless measures to limit the access of an **API** and it will not be covered in this analysis. 
Although one given idea, is to limit the access to only developer accounts, where the user is required to signup for a developer access to the **API** with appropriate reasoning and verification.

## Heuristic Measures
Below are the few ideas given for heuristic measures which can be applied:

 - **On mouse move or simulated inputs, check if start and end location of the cursor is not col-linear.**
 No human will drag/move/flick in 0.0 precision linear direction.
 
 - **On simulated inputs, check if it is near or on a target-able object**
When inputs are simulated and simulated location is near or on a target-able object, it is a 99% chance of using a input simulated script.
 
 - **If any bypass occurs for screen capturing, where such as OBS injection is abused; randomly shift RGB values of the HP Bars by few values throughout the game.** 
This is to pull the cords of pixel search based scripts where they search for the constant HP Bar RGB values.

