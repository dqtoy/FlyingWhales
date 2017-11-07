using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PathFind {
	public static class PathFind {
		public static Path<Node> FindPath<Node>(Node start, Node destination, Func<Node, Node, double> distance, Func<Node, double> estimate, PATHFINDING_MODE pathfindingMode, Kingdom kingdom = null, Region region = null) 
			where Node : HexTile, IHasNeighbours<Node> {

			var closed = new HashSet<Node>();
			var queue = new PriorityQueue<double, Path<Node>>();
			queue.Enqueue(0, new Path<Node>(start));
			Node lastStep = start;
			while (!queue.IsEmpty) {
				var path = queue.Dequeue();
				if (closed.Contains(path.LastStep))
					continue;
				if (path.LastStep.Equals(destination))
					return path;

				closed.Add(path.LastStep);
				lastStep = path.LastStep;

				double d;
				Path<Node> newPath;
                if (pathfindingMode == PATHFINDING_MODE.COMBAT) {
                    foreach (Node n in path.LastStep.CombatTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.USE_ROADS) {
                    foreach (Node n in path.LastStep.RoadTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
				} else if (pathfindingMode == PATHFINDING_MODE.MAJOR_ROADS) {
					foreach (Node n in path.LastStep.MajorRoadTiles) {
						if (n.tileTag != start.tileTag) {
							continue;
						}
						d = distance(path.LastStep, n);
						newPath = path.AddStep(n, d);
						queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
					}
				}  else if (pathfindingMode == PATHFINDING_MODE.MINOR_ROADS) {
					foreach (Node n in path.LastStep.MinorRoadTiles) {
						if (n.tileTag != start.tileTag) {
							continue;
						}
						d = distance(path.LastStep, n);
						newPath = path.AddStep(n, d);
						queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
					}
				} else if (pathfindingMode == PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM) {
					if (kingdom == null) {
						throw new Exception("Someone is trying to pathfind using MAJOR_ROADS_ONLY_KINGDOM, but hasn't specified a kingdom!");
					}
					foreach (Node n in path.LastStep.MajorRoadTiles) {
						if (n.tileTag != start.tileTag) {
							continue;
						}
						if(n.city != null && n.city.kingdom.id != kingdom.id){
							continue;
						}
						d = distance(path.LastStep, n);
						newPath = path.AddStep(n, d);
						queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
					}
				} else if (pathfindingMode == PATHFINDING_MODE.AVATAR) {
                    foreach (Node n in path.LastStep.AvatarTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.NO_HIDDEN_TILES) {
                    if (kingdom == null) {
                        throw new Exception("Someone is trying to pathfind using NO_HIDDEN_TILES, but hasn't specified a kingdom!");
                    }
                    foreach (Node n in path.LastStep.AvatarTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        if (kingdom.fogOfWarDict[FOG_OF_WAR_STATE.HIDDEN].Contains(n)) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.ROAD_CREATION) {
                    foreach (Node n in path.LastStep.RoadCreationTiles) {
                        //if (n.tileTag != start.tileTag) {
                        //    continue;
                        //}
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.LANDMARK_CREATION) {
                    foreach (Node n in path.LastStep.LandmarkCreationTiles) {
                        //if (n.tileTag != start.tileTag) {
                        //    continue;
                        //}
                        if (region != null && !region.tilesInRegion.Contains(n)) {
                            continue;
                        }
                        if (n.roadType == ROAD_TYPE.MAJOR && (n.id != start.id && n.id != destination.id)) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.NO_MAJOR_ROADS) {
                    foreach (Node n in path.LastStep.NoWaterTiles) {
                        //if (n.tileTag != start.tileTag) {
                        //    continue;
                        //}
                        if ((n.isRoad && n.roadType == ROAD_TYPE.MAJOR) || (n.isHabitable && n.id != destination.id) || n.hasLandmark) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else {
                    foreach (Node n in path.LastStep.ValidTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                }
			}
			return null;
		}
	}




}