//using System.Collections;
using NiEngine.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace NiEngine
{
    public class Saving : MonoBehaviour
    {

        public void SaveGame()
        {

            var context = new StreamContext();
            var stringOutput = new StringPrimitiveOutput();
            //TypeRegistry typeRegistry = new TypeRegistry();
            context.ThrowOnError = false;
            var output = new RootStreamOutput(stringOutput);

            output.SaveAllGameObjectsInScene(context);
            //typeRegistry.GameObjectSO.SaveGameObjects(context, gameObject, output);
            //var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            //
            //output.SaveInPlace(context, "Root", Root);
            output.SaveMetaData(context);
            var SavedDataString = stringOutput.Result;
            Debug.Log(SavedDataString);
            var filename = $"{Application.persistentDataPath}/Save.txt";
            System.IO.File.WriteAllText(filename, SavedDataString);

            StringBuilder uidObjects = new StringBuilder();
            uidObjects.AppendLine("UidObjects:");
            foreach (var (u, o) in NiEngine.UidObject.UidToObject)
            {
                uidObjects.AppendLine($"{u} : [{o.GetHashCode():X8}] : {o?.GetType().FullName}");
            }

            Debug.Log(uidObjects.ToString());


            Debug.Log(filename);

        }
        public void LoadGame()
        {
            var filename = $"{Application.persistentDataPath}/Save.txt";
            var SavedDataString = System.IO.File.ReadAllText(filename);

            var context = new StreamContext();
            var stringInput = new StringPrimitiveInput(context, SavedDataString);
            context.ThrowOnError = false;
            var input = new RootStreamInput(stringInput);
            input.LoadMetaData(context);
            input.LoadGameObjects(context);


            //var filename = $"{Application.persistentDataPath}/Save.txt";
            //var SavedDataString = System.IO.File.ReadAllText(filename);

            //var contextInput = new StreamContext();
            //var stringInput = new StringPrimitiveInput(contextInput, SavedDataString);
            //TypeRegistry typeRegistry = new TypeRegistry(stringInput);
            //var typeInput = new TypeInput(typeRegistry, stringInput);
            //IInput input = typeInput;
            //typeRegistry.LoadMetaData(contextInput, input);
            //typeRegistry.GameObjectSO.LoadGameObjects(contextInput, input);



            //input.LoadInPlace(contextInput, "Root", ref Root);


            //TypeRegistry typeRegistry = new TypeRegistry();
            //var context = new StreamContext();
            //var stringOutput = new StringPrimitiveOutput();
            //var typeOutput = new TypeOutput(typeRegistry, stringOutput);
            //IOutput output = typeOutput;
            ////typeRegistry.GameObjectSO.SaveAllGameObjectsInScene(context, output);
            //typeRegistry.GameObjectSO.SaveGameObjects(context, gameObject, output);
            ////var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            ////
            ////output.SaveInPlace(context, "Root", Root);
            //typeRegistry.SaveMetaData(context, output);
            //var SavedDataString = stringOutput.Result;
            //Debug.Log(SavedDataString);
            //var filename = $"{Application.persistentDataPath}/Save.txt";
            //System.IO.File.WriteAllText(filename, SavedDataString);

            //StringBuilder uidObjects = new StringBuilder();
            //uidObjects.AppendLine("UidObjects:");
            //foreach (var (u, o) in NiEngine.UidObject.UidToObject)
            //{
            //    uidObjects.AppendLine($"{u} : [{o.GetHashCode():X8}] : {o?.GetType().FullName}");
            //}

            //Debug.Log(uidObjects.ToString());


            //Debug.Log(filename);

        }
    }

}