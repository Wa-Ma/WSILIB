WSILIB
=======
this project:
------
Whole Slide Image + fo-dicom。
The WSILIB is seriously ordered by the supplement 145 of DICOM: Whole Slide Imaging

简介：
====
Whole Slide Image:
------
全视野数字切片(Whole Slide Image, WSI） 主要应用于病理学细胞图像领域。

**Supplement 145** 是 DICOM 关于 WSI 的重要标准，通过结合一种处理平铺大图像的方式为多帧图像和不同分辨率的多个图像提供对 WSI 的支持。主要解决了病理学图片的显示、平移、放缩、标注、显示状态等问题。是病理学图像数字化和标准化的重要标准。
+ **WSI金字塔结构**
  为了可以放缩图像，支持不同分辨率是必不可少的特性。标准 WSI使用金字塔模型来对这一特性进行支持。
  如图所示。 金字塔的不同层对应于不同的分辨率，而不同的分辨率对应的帧的数量亦不相同， 分辨率越高，对应的帧数就越多。金字塔的底部是仪器捕获的最高分辨率图像数据，而中间层是底部图像的缩略图，是其低分辨率版本，以便病理学家以低分辨率检索数据。 并且最重要的是，层与层之间的长宽一般间隔两倍，以促进快速准确的下采样
![tmp1](https://github.com/Wa-Ma/WSILIB/blob/master/introduction%20images/%E9%87%91%E5%AD%97%E5%A1%94%E7%BB%93%E6%9E%84.png)


+ **图像层次**：
DICOM 图像通过自身的 SOP Instance UID 来确定唯一的图像实例，即不同的图像有不同的 SOP Instance UID。
DICOM 图像还包含了 Series Instance UID， Series Instance UID 将不同层的图像（但是从同一最高分辨率图像取样得到的）划归为同一序列，即用同一个 Series Instance UID。**通过这种方法，将确立 WSI 图像包含哪些文件**。

+ **像素矩阵**：
在所有当前 DICOM 图像 IOD 中，像素矩阵维度存储为无符号 16 位整数，最大值为 64K 列和行，即一般大小为**(2^16^)^2^**.

+ **子图**：
子图就是存储在 DICOM 多帧图像对象中的**单独帧**。 子图可以是图像很小的一部分，这样最高层的图像（分辨率最低） 都可以储存很多帧。 或者子图可以很大，以至于金字塔的一个或多个级别仅需要一个子图。 一般选用 256×256 或者 512×512 作为子图的尺寸

+ **坐标**：
  每个帧位于相对于 WSI 的三个空间坐标： X 偏移、 Y 偏移和 Z 偏移，**一般规定左上角像素位置（X， Y） 为(1,1)** ,随着 X 的增加，帧位向下移动，随着 Y 的增加，帧位向右移动。而 Z 偏移表示这张图像处在哪一平面上，与 Z-plane 相对应.
   在同一层中，可能存在不同的图像对象，即有多个图像文件对应金字塔的一层。这时，可能即存在大子图也存在小子图，那就要求对不同的图像进行不同程度的对齐， 即对边缘区块进行填充。
+ **WSI参考框架**
![tmp2](https://github.com/Wa-Ma/WSILIB/blob/master/introduction%20images/WSI%E5%8F%82%E8%80%83%E6%A1%86%E6%9E%B6.png)


fo-dicom:
------
fo-dicom是基于.Net平台的 DICOM 库，提供了完备的 DICOM API 和完整的文档手册。

fo-dicom项目：
https://github.com/fo-dicom/fo-dicom

fo-dicom例子：
https://github.com/fo-dicomfo-dicom-samples

Spplement119:
--------
本系统还参考 Supplement 119 对帧水平获取 SOP 类的规定实现了病理学图像的 DICOM 网络传输

参考文献：
-------
+ [Supplement 145: Whole Slide Microscopic Image IOD and SOP Classes. Digital Imaging and Communications in Medicine ](http://dicom.nema.org/Dicom/DICOMWSI/)
+ [Orthanc WSI Server and Open Source Tools](https://wsi.orthanc-server.com/orthanc/app/explorer.html)
+ [DICOM](https://www.dicomstandard.org/current/)

配置：
======
老王这部分就交给你了,
1.如何配置数据库文件
2.配置好服务端