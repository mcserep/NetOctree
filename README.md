.NET Octree
===========

A dynamic octree implementation written in C# as a .NET Standard 2.1 library, built on the `System.Numerics` library.

[![Build Status](https://github.com/mcserep/NetOctree/actions/workflows/ci.yml/badge.svg)](https://github.com/mcserep/NetOctree/actions/workflows/ci.yml)
[![Code Coverage report](https://codecov.io/gh/mcserep/NetOctree/branch/master/graph/badge.svg?token=F4OY27SOC3)](https://codecov.io/gh/mcserep/NetOctree)
[![NuGet Version](https://img.shields.io/nuget/v/NetOctree)](https://www.nuget.org/packages/NetOctree/)
[![NuGet Download](https://img.shields.io/nuget/dt/NetOctree)](https://www.nuget.org/packages/NetOctree/)

How to get it?
-----------

The easiest way to get **NetOctree** is to install from the [NuGet package manager](https://docs.microsoft.com/hu-hu/nuget/install-nuget-client-tools).

Use the Package Manager Console:
```
PM> Install-Package NetOctree
```

Or the `dotnet` command line interface:
```
dotnet add package NetOctree
```

Description
-----------

There are two octree implementations here:    
**BoundsOctree** stores any type of object, with the object boundaries defined as an axis-aligned bounding box. It's a dynamic octree and can also be a loose octree.   
**PointOctree** is the same basic implementation, but stores objects as a point in space instead of bounds. This allows some simplification of the code. It's a dynamic octree as well.

**Octree:** An octree a tree data structure which divides 3D space into smaller partitions (nodes) and places objects into the appropriate nodes. This allows fast access to objects in an area of interest without having to check every object.

**Dynamic:** The octree grows or shrinks as required when objects are added or removed. It also splits and merges nodes as appropriate. There is no maximum depth. Nodes have a constant (*numObjectsAllowed*) which sets the amount of items allowed in a node before it splits.

**Loose:** The octree's nodes can be larger than 1/2 their parent's length and width, so they overlap to some extent. This can alleviate the problem of even tiny objects ending up in large nodes if they're near boundaries. A looseness value of 1.0 will make it a "normal" octree.

**A few functions are implemented:**

With `BoundsOctree`, you can pass in bounds and get a true/false answer for if it's colliding with anything (`IsColliding`), or get a list of everything it's collising with (`GetColliding`).
With `PointOctree`, you can cast a ray and get a list of objects that are within x distance of that ray (`GetNearby`). You may also get a list of objects that are within x distance from a specified origin point.

It shouldn't be too hard to implement additional functions if needed. For instance, PointOctree could check for points that fall inside a given bounds.

**Considerations:**

Tree searches are recursive, so there is technically the potential for a stack overflow on very large trees. The `minNodeSize` parameter limits node side and hence the depth of the tree, putting a cap on recursion.

I tried switching to an iterative solution using my own stack, but creating and manipulating the stack made the results generally slower than the simple recursive solution. However, I wouldn't be surprised it someone smarter than me can come up with a faster solution.

Another note: You may notice when viewing the bounds visualisation that the child nodes' outer edges are all inside the parent nodes. But loose octrees are meant to make the inner nodes bigger... aren't they? The answer is yes, but the parent nodes are *also* bigger, and e.g. ((1.2 * 10) - 10) is bigger than ((1.2 * 5) - 5), so the parent node ends up being bigger overall.

This seems to be the standard way that loose octrees are done. I did an experiment: I tried making the child node dimensions looseness * the parent's actual size, instead of looseness * the parent's base size before looseness is applied. This seems more intuitively correct to me, but performance seems to be about the same.

Example Usage
-----------

**Create an Octree**

```C#
// Initial size (metres), initial centre position, minimum node size (metres), looseness
BoundsOctree<DataType> boundsTree = new BoundsOctree<DataType>(15, position, 1, 1.25f);
// Initial size (metres), initial centre position, minimum node size (metres)
PointOctree<DataType> pointTree = new PointOctree<DataType>(15, position, 1);
```

- The initial size should ideally cover an area just encompassing all your objects. If you guess too small, the octree will grow automatically, but it will be eight times the size (double dimensions), which could end up covering a large area unnecessarily. At the same time, the octree will be able to shrink down again if the outlying objects are removed. If you guess an initial size that's too big, it won't be able to shrink down, but that may be the safer option. Don't worry too much: In reality the starting value isn't hugely important for performance.
- The initial position should ideally be in the centre of where your objects are.
- The minimum node size is effectively a depth limit; it limits how many times the tree can divide. If all your objects are e.g. 1m+ wide, you wouldn't want to set it smaller than 1m.
- The best way to choose a looseness value is to try different values (between 1 and maybe 1.5) and check the performance with your particular data. Generally around 1.2 is good.

**Add and Remove**

```C#
boundsTree.Add(myObject, myBounds);
boundsTree.Remove(myObject);

pointTree.Add(myObject, myVector3);
boundsTree.Remove(myObject);
```
- The object's type depends on the tree's type.
- The bounds or point determine where it's inserted.

**Search in the Octree**

```C#
bool isColliding = boundsTree.IsColliding(bounds);
```

```C#
DataType[] collidingWith = boundsTree.GetColliding(bounds);
```
- Where `DataType` is the type of the octree

```C#
DataType[] nearby = pointTree.GetNearby(myRay, 4);
```
- Where `myRay` is a `Ray`
- In this case we're looking for any point within 4m of the closest point on the ray

```C#
DataType[] nearby = pointTree.GetNearby(myPos, 4);
```
- Where `myPos` is a `Vector3` from `System.Numerics`

**Non-Alloc query functions**

A pre-initialized list can be used to store the results, which can be useful when executing a large number of queries with potentially large result sets.

```C#
List<DataType> collidingWith = new List<DataType>();
boundsTree.GetColliding(collidingWith, bounds);
```

```C#
pointTree.GetNearby(myRay, 4, collidingWith);
```

**Potential Improvements**

A significant portion of the octree's time is taken just to traverse through the nodes themselves. There's potential for a performance increase there, maybe by linearising the tree - that is, representing all the nodes as a one-dimensional array lookup.
