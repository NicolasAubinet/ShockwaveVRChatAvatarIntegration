This is a Unity plugin to easily integrate Shockwave suit compatible haptics to your VRChat avatar.

In your avatar Unity project, start byadding a package from this git URL: https://github.com/NicolasAubinet/ShockwaveVRChatAvatarIntegration.git

![image](https://github.com/NicolasAubinet/ShockwaveVRChatAvatarIntegration/assets/4831228/7d9e4beb-4d8b-42d2-a7a5-148bc8cdd233) ![image](https://github.com/NicolasAubinet/ShockwaveVRChatAvatarIntegration/assets/4831228/76f22b72-4166-44ce-bfc9-5dc487ac0995)


You'll then have a new Shockwave menu when right clicking on your avatar. Just click on "Add haptics to avatar" to add all the required colliders.

![image](https://github.com/NicolasAubinet/ShockwaveVRChatAvatarIntegration/assets/4831228/33792770-7a47-455d-94d7-7a9bf641092b)

You will probably have to adjust the haptic sections positions and scales to match your avatar's metrics (you can search for "shockwave colliders" in the hierarchy to find all added haptic sections).

By default, haptics only happen when there is a contact with another avatar, but feel free to test self contact with the "Enable self-collision" (note that self-collisions have a performance impact due to increased contact receivers count).

To actually trigger haptics, you need to run this app on your PC https://github.com/NicolasAubinet/ShockwaveVRChat
