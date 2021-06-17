# EnvironmentMapping
This is a project developed during Final Year Project as BSc of Games Design & Development.<br/>
The project uses Augmented Reality on Android devices in combination with Point Cloud to scan the user's physical space.<br/>
This Point Cloud data can be saved on mobile device and brought into the Virtual Reality.<br/>
In Virtual Reality, the Point Cloud data is recreated and sorted through using Octrees to create simple visual representation of the environment scanned.<br/>

Video on how this project works can be found below:<br/>
[YouTube Link](https://youtu.be/IUi1OBRkUDs)<br/><br/>


Main Features:
1. Point Cloud data [breaking down](https://github.com/DatPinkGuy/EnvironmentMapping/blob/b03e03e38c31d151c0defbd20a924e929caded99/EnvironmentMappingARVR/Assets/Alex/Scripts/GetPointPos.cs#L108) into saveable format.
2. [Saving](https://github.com/DatPinkGuy/EnvironmentMapping/blob/b03e03e38c31d151c0defbd20a924e929caded99/EnvironmentMappingARVR/Assets/Alex/Scripts/SaveSystem.cs#L19) and [Loading](https://github.com/DatPinkGuy/EnvironmentMapping/blob/b03e03e38c31d151c0defbd20a924e929caded99/EnvironmentMappingARVR/Assets/Alex/Scripts/SaveSystem.cs#L78) of Point Cloud Data.
3. Octree [creation](https://github.com/DatPinkGuy/EnvironmentMapping/blob/b03e03e38c31d151c0defbd20a924e929caded99/EnvironmentMappingARVR/Assets/Alex/Scripts/MeshPointLoading.cs#L117) based on Point Cloud data provided.
4. [Sorting algorithm](https://github.com/DatPinkGuy/EnvironmentMapping/blob/b03e03e38c31d151c0defbd20a924e929caded99/EnvironmentMappingARVR/Assets/Alex/Scripts/MeshPointLoading.cs#L165) to locate smallest nodes in Octree with specified amount of points in them.
5. [Methods](https://github.com/DatPinkGuy/EnvironmentMapping/blob/b03e03e38c31d151c0defbd20a924e929caded99/EnvironmentMappingARVR/Assets/Alex/Scripts/SyncPosition.cs#L27) to allow user for manual position synchronization in VR.<br/><br/>

Preview:<br/>
