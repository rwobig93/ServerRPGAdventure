   M S B A M L   `   `   `  ����T  PPersonalDiscordBot, Version=0.1.6634.34681, Culture=neutral, PublicKeyToken=nullG(clr-namespace:PersonalDiscordBot.WindowsPersonalDiscordBot.Windows  5   	   .    (PersonalDiscordBot.Windows.Server_Editor   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   :local(clr-namespace:PersonalDiscordBot.Windows     5   	   -   5          � Title$  Server_Editor��5   	   $	��485��6   $	��889��6,    � WindowStyle$
 None=�68    � AllowsTransparency . ���	#FF202020.��5	      � -   5         �Name$ grdMain��5	      ���	#FF202020.+�5
   
   �� -   5         ��Name$ lblServGame��5
      $
��Left=�6)   $��	10,45,0,0q�6D   $
��Wrap=�6W   $��
Game Name:��6k   $	��Top=�6}   ���	#FF0FB3C3$��22(�6�   �� 5   
   -   5         ��Name$ txtServGameValue��5      $��
144,49,0,0q�6,   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $	��735��6�   $��1��6�   �� 5   
   -   5        $ lblServName��5      $
��Left=�6)   $��	10,79,0,0q�6D   $
��Wrap=�6W   $��Server Name:��6k   $	��Top=�6   ���	#FF0FB3C3$��22(�6�   �� 5   
   -   5        $ txtServNameValue��5      $��
144,83,0,0q�6,   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $	��735��6�   $��2��6�   �� 5   
   -   5        $ lblServPass��5      $
��Left=�6)   $��
10,113,0,0q�6D   $
��Wrap=�6X   $��
*Password:��6l   $	��Top=�6~   ���	#FF0FB3C3$��22(�6�   $	��107��6�   �� 5   
   -   5        $ txtServPassValue��5      $��122,117,0,0q�6,   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $	��757��6�   $��3��6�   �� 5   
   -	   5        $ 	lblServIP��5      $
��Left=�6'   $��
10,147,0,0q�6B   $
��Wrap=�6V   $��IP Address:��6j   $	��Top=�6}   ���	#FF0FB3C3$��22(�6�   �� 5   
   -
   5        $ txtServIPValue��5      $��122,151,0,0q�6*   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $	��152��6�   $��4��6�   �� 5   
   -   5        $ lblServPort��5      $
��Left=�6)   $��287,148,0,0q�6D   $
��Wrap=�6Y   $��Game Port #:��6m   $	��Top=�6�   ���	#FF0FB3C3$��22(�6�   �� 5   
   -   5        $ txtServPortValue��5      $��418,150,0,0q�6,   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $��74��6�   $��5��6�   �� 5   
   -   5        $ lblServModded��5      $
��Left=�6+   $��716,150,0,0q�6F   $
��Wrap=�6[   $��Modded:��6o   $	��Top=�6~   ���	#FF0FB3C3$��22(�6�   �� 5   
   -   5         ��Name$ comboServModded��5       �� 
IsEditable . $
��Left=�6>    �� Text$ False��6Y   $��809,151,0,0q�6f   $	��Top=�6{   $��70��6�   $��29��6�   $��18(�6�   $��0��6�   $��0��6�   ���	#FF0FB3C3$��7��6�   .@�5      �� $
��True��6   $��18(�6*   �� 5      $��False��6   $��18(�6+   5      �� 5   
   -   5        	 ��Name$	 btnServSave��5      $��Apply��6@   $��0q�6P   $��1q�6\   $��Right=�6p   $��Bottom=�6�   $��88��6�   $��32��6�   ���	#FF202020���	#FF6E6E6E���	#FF0FB3C3$��20(�6  $��
0,0,103,94q�6  $��13��6&  �� 5   
   -   5        $	 btnServCancel��5      $��Cancel��6D   $��0q�6U   $��1q�6a   $��Right=�6u   $��Bottom=�6�   $��88��6�   $��32��6�   ���	#FF202020���	#FF6E6E6E���	#FF0FB3C3$��20(�6	  $��	0,0,10,94q�6  $��14��6*  �� 5   
   -   5        $ lblServNotice��5      $��
10,0,10,10q�6+   $
��Wrap=�6?   $��Bottom=�6S   ���	#FF474D4D$	��116��6�   
 K� ZIndex$
 -1��6�   �� 6�   $���* = Optional feature/field, features turned off if nothing is entered in the field, if the server doesn't have a password leave blank��6�   �� 68  �� 6D  $^�X*The password field is meant for the password to join the server, not the admin password��6H  �� 6�  �� 6�  $|�v*Query Port # is used to gather server and player information including up status, leaving blank results in basic info��6�  �� 6:  �� 6F  $���*Server Exe is the full file path to the EXE that is run when the game server starts (the exe called by a batch file or clicked directly)��6J  �� 6�  �� 6�  $O�I*Batch/Exe to Start Server is the full file path to start the game server��6�  �� 6@  �� 6L  $=�7*Process is the process name of the running game server��6P  �� 6�  �� 6�  $e�_*Server Log Location is the directory the game server logs reside, add if you want this feature��6�  6  �� 5   
   -   5        $ lbServExePath��5      $
��Left=�6+   $��
10,216,0,0q�6F   $
��Wrap=�6Z   $��*Server Exe Path:��6n   $	��Top=�6�   ���	#FF0FB3C3$��22(�6�   �� 5   
   -   5        $ txtServExePath��5      $��177,219,0,0q�6*   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $	��702��6�   $��9��6�   �� 5   
   -   5        $ lblServLogLocation��5      $
��Left=�60   $��
10,322,0,0q�6K   $
��Wrap=�6_   $��*Server Log Location:��6s   $	��Top=�6�   ���	#FF0FB3C3$��22(�6�   �� 5   
   -   5        $ txtServLogLocationValue��5      $��220,322,0,0q�63    �� TextWrapping$ NoWrap=�6H   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $	��659��6�   $��12��6�   �� 5    
   -   5        $ lbServProcess��5       $
��Left=�6+   $��
10,288,0,0q�6F   $
��Wrap=�6Z   $��*Process Name:��6n   $	��Top=�6�   ���	#FF0FB3C3$��22(�6�   �� 5!   
   -   5        $ txtServProcessValue��5!      $��166,288,0,0q�6/   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $	��713��6�   $��11��6�   �� 5"   
   ���	#FF191919$��39��6%   $	��Top=�61   �� 5#   
   -   5        $	 btnClose��5#      $��X��6:   $��0q�6F   $��1q�6R   $��Right=�6f   $	��Top=�6�   $��32��6�   $��32��6�   ���	#FF202020���	#FF6E6E6E���	#FF0FB3C3$��20(�6�   $��14��6  �� 5$   
   -   5        $ lblServGame_Copy��5$      $
��Wrap=�6.   $��Server Editor��6B   $	��Top=�6W   ���	#FF474D4D$��22(�6�   $	��126��6�   $��39��6�   $��Center=�6�   �� 5%   
   -   5        $ lblExtHostname��5%      $
��Left=�6,   $��
10,181,0,0q�6G   $
��Wrap=�6[   $$��*External HostName/IP Address:��6o   $	��Top=�6�   ���	#FF0FB3C3$��22(�6�   �� 5&   
   -   5        $ txtExtHostnameValue��5&      $��326,185,0,0q�6/   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $	��553��6�   $��8��6�   �� 5'   
   -   5        $ lbServStartPath��5'      $
��Left=�6-   $��
10,250,0,0q�6H   $
��Wrap=�6\   $!��*Batch/Exe to Start Server:��6p   $	��Top=�6�   ���	#FF0FB3C3$��22(�6�   �� 5(   
   -   5        $ txtServStartPathValue��5(      $��268,253,0,0q�61   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $	��611��6�   $��10��6�   �� 5)   
   -   5        $ lblQueryPort��5)      $
��Left=�6*   $��495,148,0,0q�6E   $
��Wrap=�6Z   $��*Query Port #:��6n   $	��Top=�6�   ���	#FF0FB3C3$��22(�6�   �� 5*   
   -   5        $ txtQueryPortValue��5*      $��637,150,0,0q�6-   ���	#FF202020���	#FF0FB3C3���	#FF6E6E6E$��29��6�   $	��Top=�6�   $��18(�6�   $
��Left=�6�   $��74��6�   $��6��6�   5+      5,      