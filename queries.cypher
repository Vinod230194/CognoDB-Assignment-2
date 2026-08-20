-- 1-hop: who knows whom
MATCH (a:Person)-[:KNOWS]->(b:Person)
RETURN a.name AS from, b.name AS to, b.age AS age;

-- Multi-hop: friends-of-friends (length 2)
MATCH p=(a:Person)-[:KNOWS*2]->(b:Person)
RETURN p LIMIT 25;

-- Parameterized: find people known by a given name
MATCH (a:Person {name:$name})-[:KNOWS]->(b:Person)
RETURN b.name AS friendName, b.age AS friendAge;

-- Path length and shortest path example
MATCH (a:Person {name:$n1}), (b:Person {name:$n2})
RETURN shortestPath((a)-[:KNOWS*..5]->(b)) AS path;
