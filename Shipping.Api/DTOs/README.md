similar to zod type validations what user should send 
We DON'T want the frontend sending an entire database model.
bad way generallly
{
  "id": 928,
  "trackingNumber": "whatever",
  "status": "Delivered",
  "userId": 123,
  "destination": "Delhi"
}

Customer shouldn't decide these things.

Instead: we create these requests DTOs/CreateShipmentRequest.cs n all so customer only decide
{
  "destination": "Amsterdam",
  "carrierId": 1
}

then backend decides

UserId          ← JWT
TrackingNumber  ← backend
Status          ← Created
CreatedAt       ← backend
That's why DTOs exist.

DTO ≈ request type / Zod input schema