using UnityEngine;
using Python.Runtime;
using System;
using UnityEngine.InputSystem;

public class ModelHandler : MonoBehaviour
{
    private PyModule scope;
    public GameObject eye;
    public string modelPath;
    public string weightsPath;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Init pyhton
        //string dllPath = @"C:\Users\matus\AppData\Local\Programs\Python\Python312\python312.dll";
        string dllPath = "C:\\Users\\" + Environment.UserName.ToLower() + "\\AppData\\Local\\Programs\\Python\\Python312\\python312.dll";
        Debug.Log(dllPath.ToString());
        Runtime.PythonDLL = dllPath;
        PythonEngine.Initialize();
        scope = Py.CreateScope();
        using (Py.GIL())
        {
            scope.Exec("import tensorflow as tf");
            scope.Exec("import keras as kr");
            scope.Exec("import cv2 as cv");
            scope.Exec("import numpy as np");
            //scope.Set("model_path", "C:\\bp\\BP\\Assets\\StreamingAssets\\" + modelPath);
            string model_path = "./" + modelPath;
            Debug.Log(model_path);
            scope.Set("model_path", new PyString(model_path));
            scope.Exec("model = kr.models.load_model(model_path)");
            scope.Exec("model.compile()");
            scope.Exec("cam = cv.VideoCapture(0)");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            //UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
        
        using (Py.GIL())
        {
            scope.Exec("ret, frame = cam.read()");           
            scope.Exec("pred_input = cv.cvtColor(frame, cv.COLOR_BGR2RGB)");
            scope.Exec("pred_input = cv.resize(pred_input, (60, 60))");
            scope.Exec("pred_input = tf.expand_dims(pred_input, 0)");
            //scope.Exec("pred_ = model.predict(tf.convert_to_tensor(pred_input, dtype=tf.uint8))");
            scope.Exec("pred_ = model.predict(np.asarray(pred_input, dtype=np.uint8))");
            scope.Exec("out_ = np.argmax(pred_)");
            scope.Exec("match out_:\r\n        case 0:\r\n            out_ = \"down\"\r\n        case 1:\r\n            out_ =  \"front\"\r\n        case 2:\r\n            out_ =  \"left\"\r\n        case 3:\r\n            out_ =  \"none\"\r\n        case 4:\r\n            out_ =  \"right\"\r\n        case 5:\r\n            out_ =  \"up\"");
            scope.Exec("cv.putText(frame, out_, (5, 60), cv.FONT_HERSHEY_SIMPLEX, 2, (0, 255, 0), 3, cv.LINE_AA)");
            scope.Exec("cv.imshow('frame', frame)");
            string prediction = scope.Get("out_").ToString();
            switch (prediction)
            {
                case "front":
                    eye.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                    break;

                case "down":
                    eye.transform.eulerAngles = new Vector3(0.0f, 0.0f, -45.0f);
                    break;

                case "up":
                    eye.transform.eulerAngles = new Vector3(0.0f, 0.0f, 45.0f);
                    break;

                case "right":
                    eye.transform.eulerAngles = new Vector3(0.0f, 45.0f, 0.0f);
                    break;

                case "left":
                    eye.transform.eulerAngles = new Vector3(0.0f, -45.0f, 0.0f);
                    break;

                case "none":
                default:
                    break;

            }
        }
    }

    private void OnDestroy()
    {
        using (Py.GIL())
        {
            scope.Exec("cam.release()");
            scope.Exec("cv.destroyAllWindows()");
        }
        PythonEngine.Shutdown();
    }
}
