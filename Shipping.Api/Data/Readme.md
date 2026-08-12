// ef core
Now we need to connect these C# classes to SQL Server.
comparing to the ecosystem postgres to sqlserver with prisma and ef core.
 
schema.prisma => C# Models
PrismaClient => DBContext
prisma.shipment => db.Shipments

it's liek a prisma client we can import & work on it real quick
import { PrismaClient } from "@prisma/client";

  const prisma = new PrismaClient();

  await prisma.user.findMany();
  await prisma.shipment.create(...);
/ AppDbContext is injected as _db

  await _db.Users.ToListAsync();
  _db.Shipments.Add(shipment);
  await _db.SaveChangesAsync();