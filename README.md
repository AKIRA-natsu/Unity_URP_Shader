# Unity_URP_Shader

Unity 2022.3.2 Shader学习与实现

[径向模糊](https://zhuanlan.zhihu.com/p/542680959)

1. 已经过时的字段，需要改成cameraColorTargetHandler

```csharp
renderer.cameraColorTarget
```

2. 报错提示
```csharp
// You can only call cameraColorTarget inside the scope of a ScriptableRenderPass. Otherwise the pipeline camera target texture might have not been created or might have already been disposed.
// m_ScriptablePass.setupmypass(renderer.cameraColorTarget);

// 改成RenderPass内部重写OnCameraSetup
public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
    base.OnCameraSetup(cmd, ref renderingData);
    this.Source = renderingData.cameraData.renderer.cameraColorTargetHandle;
}
```

效果图：
![img](./Assets/Res/Textures/1.png)
![img](./Assets/Res/Textures/2.png)