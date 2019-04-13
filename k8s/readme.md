
dotnetclub Kubernetes 安装指南
========


此目录下的 `admin` 和 `app` 两个子目录可帮助你在 k8s 环境中运行一个包含多个运行实例副本的 dotnetclub 应用。

这两个目录的作用是：

* `admin` 目录是一些供集群管理员使用的基础配置
* `app` 目录是运行 dotnetclub 应用所需的 Kubernetes 配置

这两个目录中的 yaml 文件的使用流程是：使用 kubectl 按照序号顺次执行各个 yaml 文件。在 `admin` 目录中，序号为 02 的配置文件有两个，只要二选其一即可。

`admin` 子目录各文件的作用为：

* 00-namespace: 创建名称为 `dotnetclub` 的名称空间
* 01-limit-range: （可选）在名称空间里，创建标准的资源限制情况
* **02-gluster-storage-class**: 创建一个动态存储类，以供动态供给存储卷（与 02-persistent-volume 二选其一；在执行之前需要根据实际情况完成编辑，请注意其中的注释说明）
* **02-persistent-volume**: 创建静态的 glusterfs 存储卷 （与 02-gluster-storage-class 二选其一；在执行之前需要根据实际情况完成编辑，请注意其中的注释说明）

关于存储的注意：
1. 上述两个 02 配置要求你自己提供用于存储的 glusterfs 集群。如果要使用 02-gluster-storage-class，你需要一个配备了 Heketi 的 glusterfs 集群。
1. 上面的两个 02 只需要执行一种即可。
1. 如果你的集群中，已经具备其他动态或静态存储卷，可跳过所有这两个 02 存储卷（类）的创建。
1. 如果你使用 nfs、AzureDisk 等其他类型的存储设施，请自行完成对应存储设施配置的编写。



`app` 子目录各文件的作用为：

* 06-persistent-volume-claim: 为 dotnetclub 声明要使用的存储设施（如果需要，请根据你实际的存储设施完成编辑）
* 07-config-map: dotnetclub 应用所需的配置信息（如果需要，可以自行调整其中各项的值）
* **08-secret**: dotnetclub 应用所需的加解密密钥（在运行之前，要注意其中的生效、失效日期；请自行重新生成新的密钥并完成替换）
* 09-replica-set: 声明 dotnetclub 应用的运行方式
* 10-service: 把 dotnetclub 网站暴露到集群之外，从而能够公开访问


在运行之前，请对上面加粗的各个文件按照你实际情形完成更新。否则，dotnetclub 将无法在你的 Kubernetes 中成功运行。
