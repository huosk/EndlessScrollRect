# UGUI ScrollRect 优化

基于 LayoutGroup 实现 ScrollRect 滚动视图的优化。实现中采用双向链表管理 UI 元素，通过保证链表的有序性，来提高 UI 排序效率。

![demo](Img/cap1.gif)