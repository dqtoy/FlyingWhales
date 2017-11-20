using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PathFind {
	public static class PathFind {
		public static Path<Node> FindPath<Node>(Node start, Node destination, Func<Node, Node, double> distance, Func<Node, double> estimate, PATHFINDING_MODE pathfindingMode, Kingdom kingdom = null) 
			where Node : HexTile, IHasNeighbours<Node> {

			var closed = new HashSet<Node>();
			var queue = new PriorityQueue<double, Path<Node>>();
			queue.Enqueue(0, new Path<Node>(start));
			Node lastStep = start;

            Region region1 = start.region;
            Region region2 = destination.region;

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
                if(pathfindingMode == PATHFINDING_MODE.REGION_CONNECTION) {
                    foreach (Node n in path.LastStep.RegionConnectionTiles) {
                        if(n.region.id != region1.id && n.region.id != region2.id) {
                            //path cannot pass through other regions
                            continue;
                        }
                        if(n.allNeighbourRoads.Count > 0 && n.id != start.id && n.id != destination.id) {
                            //current node has adjacent roads, check if it is a neighbour of start or destination
                            //if it is, allow the path
                            //else skip this node
                            if(!start.AllNeighbours.Contains(n) && !destination.AllNeighbours.Contains(n)) {
                                continue;
                            }
                        }

                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.LANDMARK_CONNECTION) {
                    foreach (Node n in path.LastStep.LandmarkConnectionTiles) {
                        if (n.region.id != region1.id && n.region.id != region2.id) {
                            //path cannot pass through other regions
                            continue;
                        }
                        if (n.MinorRoadTiles.Count > 0 && n.id != start.id && n.id != destination.id) {
                            continue;
                        }
                        if (n.isHabitable && n.id != start.id && n.id != destination.id) {
                            continue;
                        }
                        if (n.hasLandmark && n.id != start.id && n.id != destination.id) {
                            continue;
                        }
                        //if (n.RoadTiles.Count > 0 && n.id != start.id && n.id != destination.id) {
                        //    //current node has adjacent roads, check if it is a neighbour of start or destination
                        //    //if it is, allow the path
                        //    //else skip this node
                        //    if (!start.AllNeighbours.Contains(n) && !destination.AllNeighbours.Contains(n)) {
                        //        continue;
                        //    }
                        //}

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
						if(n.id != start.id && n.city != null && n.city.kingdom.id != kingdom.id){
							continue;
						}
						d = distance(path.LastStep, n);
						newPath = path.AddStep(n, d);
						queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
					}
				} else if (pathfindingMode == PATHFINDING_MODE.MINOR_ROADS_ONLY_KINGDOM) {
					if (kingdom == null) {
						throw new Exception("Someone is trying to pathfind using MINOR_ROADS_ONLY_KINGDOM, but hasn't specified a kingdom!");
					}
					foreach (Node n in path.LastStep.MinorRoadTiles) {
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
				} else if (pathfindingMode == PATHFINDING_MODE.USE_ROADS) {
					foreach (Node n in path.LastStep.allNeighbourRoads) {
						if (n.tileTag != start.tileTag) {
							continue;
						}
						d = distance(path.LastStep, n);
						newPath = path.AddStep(n, d);
						queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
					}
				}  else if (pathfindingMode == PATHFINDING_MODE.USE_ROADS_WITH_ALLIES) {
					if (kingdom == null) {
						throw new Exception("Someone is trying to pathfind using USE_ROADS_WITH_ALLIES, but hasn't specified a kingdom!");
					}
					foreach (Node n in path.LastStep.allNeighbourRoads) {
						if (n.tileTag != start.tileTag) {
							continue;
						}
						if (n.city != null && n.city.kingdom.id != kingdom.id) {
							KingdomRelationship kr = n.city.kingdom.GetRelationshipWithKingdom (kingdom);
							if (!kr.AreAllies ()) {
								continue;
							}
						}
						d = distance(path.LastStep, n);
						newPath = path.AddStep(n, d);
						queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
					}
				} else if (pathfindingMode == PATHFINDING_MODE.USE_ROADS_ONLY_KINGDOM) {
					if (kingdom == null) {
						throw new Exception("Someone is trying to pathfind using USE_ROADS_ONLY_KINGDOM, but hasn't specified a kingdom!");
					}
					foreach (Node n in path.LastStep.allNeighbourRoads) {
						if (n.tileTag != start.tileTag) {
							continue;
						}
						if (n.city != null && n.city.kingdom.id != kingdom.id) {
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
						d = distance (path.LastStep, n);
						newPath = path.AddStep (n, d);
						queue.Enqueue (newPath.TotalCost + estimate (n), newPath);
					}
                } else if (pathfindingMode == PATHFINDING_MODE.LANDMARK_EXTERNAL_CONNECTION) {
                    foreach (Node n in path.LastStep.LandmarkExternalConnectionTiles) {
                        if (n.region.id != region1.id && n.region.id != region2.id) {
                            //path cannot pass through other regions
                            continue;
                        }
                        if(n.AllNeighbourRoadTiles.Where(x => x.roadType == ROAD_TYPE.MINOR).Count() > 0 && n.id != start.id && n.id != destination.id) {
                            continue;
                        }
                        if (n.isHabitable && n.id != start.id && n.id != destination.id) {
                            continue;
                        }
                        if (n.hasLandmark && n.id != start.id && n.id != destination.id) {
                            continue;
                        }
                        //if (n.RoadTiles.Count > 0 && n.id != start.id && n.id != destination.id) {
                        //    //current node has adjacent roads, check if it is a neighbour of start or destination
                        //    //if it is, allow the path
                        //    //else skip this node
                        //    if (!start.AllNeighbours.Contains(n) && !destination.AllNeighbours.Contains(n)) {
                        //        continue;
                        //    }
                        //}

                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.UNIQUE_LANDMARK_CREATION) {
                    foreach (Node n in path.LastStep.LandmarkConnectionTiles) {
                        //if (n.region.id != region1.id && n.region.id != region2.id) {
                        //    //path cannot pass through other regions
                        //    continue;
                        //}
                        if (n.isRoad && n.id != start.id && n.id != destination.id) {
                            continue;
                        }
                        if (n.isHabitable && n.id != start.id && n.id != destination.id) {
                            continue;
                        }
                        if (n.hasLandmark && n.id != start.id && n.id != destination.id) {
                            continue;
                        }
                        //if (n.RoadTiles.Count > 0 && n.id != start.id && n.id != destination.id) {
                        //    //current node has adjacent roads, check if it is a neighbour of start or destination
                        //    //if it is, allow the path
                        //    //else skip this node
                        //    if (!start.AllNeighbours.Contains(n) && !destination.AllNeighbours.Contains(n)) {
                        //        continue;
                        //    }
                        //}

                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.COMBAT) {
                    foreach (Node n in path.LastStep.CombatTiles) {
                        if (n.tileTag != start.tileTag) {
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