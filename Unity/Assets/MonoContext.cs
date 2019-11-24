using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MonoContext : MonoBehaviour {

    IEnumerator Start() {
#if UNITY_WEBGL && !UNITY_EDITOR
		RegisterPlugin();
#endif
        CreateBuffer();
        yield return StartCoroutine("CallPluginAtEndOfFrames");
    }

    // We'll pass native pointer to the mesh vertex buffer.
    // Also passing source unmodified mesh data.
    // The plugin will fill vertex data from native code.
#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RenderingPlugin")]
#endif
    private static extern void SetMeshBuffersFromUnity(IntPtr vertexBuffer, int vertexCount, IntPtr sourceVertices, IntPtr sourceNormals, IntPtr sourceUVs);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RenderingPlugin")]
#endif
    private static extern void SetTimeFromUnity(float t);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RenderingPlugin")]
#endif
    private static extern IntPtr GetRenderEventFunc();

    private void CreateBuffer() {

        GameObject sphere = GameObject.Find("Sphere");

        var filter = sphere.GetComponent<MeshFilter>();
        var mesh = filter.mesh;
        // The plugin will want to modify the vertex buffer -- on many platforms
        // for that to work we have to mark mesh as "dynamic" (which makes the buffers CPU writable --
        // by default they are immutable and only GPU-readable).
        mesh.MarkDynamic();

        // Make sure to have vertex colors so that the plugin can rely on a known
        // vertex layout (position+normal+color+UV). Since Unity 2019.3 it's easier
        // since there are APIs to query all that info.
        var colors = mesh.colors;
        mesh.colors = colors;

        // However, mesh being dynamic also means that the CPU on most platforms can not
        // read from the vertex buffer. Our plugin also wants original mesh data,
        // so let's pass it as pointers to regular C# arrays.
        // This bit shows how to pass array pointers to native plugins without doing an expensive
        // copy: you have to get a GCHandle, and get raw address of that.
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var uvs = mesh.uv;
        GCHandle gcVertices = GCHandle.Alloc(vertices, GCHandleType.Pinned);
        GCHandle gcNormals = GCHandle.Alloc(normals, GCHandleType.Pinned);
        GCHandle gcUV = GCHandle.Alloc(uvs, GCHandleType.Pinned);

        SetMeshBuffersFromUnity(mesh.GetNativeVertexBufferPtr(0), mesh.vertexCount, gcVertices.AddrOfPinnedObject(), gcNormals.AddrOfPinnedObject(), gcUV.AddrOfPinnedObject());

        gcVertices.Free();
        gcNormals.Free();
        gcUV.Free();
    }

    private IEnumerator CallPluginAtEndOfFrames() {
        while (true) {
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();

            // Set time for the plugin
            SetTimeFromUnity(Time.timeSinceLevelLoad);
<<<<<<< Updated upstream

=======
            
>>>>>>> Stashed changes
            // Issue a plugin event with arbitrary integer identifier.
            // The plugin can distinguish between different
            // things it needs to do based on this ID.
            // For our simple plugin, it does not matter which ID we pass here.
            GL.IssuePluginEvent(GetRenderEventFunc(), 1);
        }
    }
}
