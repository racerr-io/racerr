[Input]
Since the desktop input manager uses GetAxis() and GetButton() to avoid hard-coding the inputs, you will also need to set up those.
More about this can be found inside the manual, but for a quick start you can just copy the provided project settings (ProjectSettings.zip) 
into your ProjectSettings directory ([your_project_name]/ProjectSettings). This will overwrite your existing settings.
If you do not set up the inputs desktop input manager will still work but will show warnings and use hard-coded defaults.

[Triggers]
If you do not want wheels to detect colliders that are triggers, untick "Queries Hit Triggers" in Project Settings > Physics.

[Collisions]
If you want vehicles to colide with inactive vehicles and other kinematic rigidbodies set Contact Pairs Mode
to "Enable All Contact Pairs" under Project Settings > Physics.