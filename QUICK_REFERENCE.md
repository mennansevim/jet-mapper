# 🚀 FastMapper - Quick Reference

## 📋 API Reference Table

| Method | Description | Example |
|--------|-------------|---------|
| `Builder()` | Start mapping builder | `user.Builder()` |
| `MapTo<T>()` | Set target type | `.MapTo<UserDto>()` |
| `Set()` | Assign property value | `.Set(d => d.Name, s => s.FullName)` |
| `SetIf()` | Conditional assignment | `.SetIf(d => d.Status, s => s.IsActive, s => "Active")` |
| `SetIfElse()` | If-else logic | `.SetIfElse(d => d.Contact, (d => d.Email, s => s.Email), ...)` |
| `Ignore()` | Skip property | `.Ignore(d => d.Password)` |
| `BeforeMap()` | Pre-mapping hook | `.BeforeMap((src, dest) => {...})` |
| `AfterMap()` | Post-mapping hook | `.AfterMap((src, dest) => {...})` |
| `Create()` | Complete mapping | `.Create()` |
| `Create(target)` | Map to existing object | `.Create(existingDto)` |

## 🎯 Common Patterns

### 1. Basic Mapping
```csharp
var dto = source.FastMapTo<TargetDto>();
```

### 2. Custom Property
```csharp
var dto = person.Builder()
    .MapTo<PersonDto>()
    .Set(d => d.FullName, p => $"{p.FirstName} {p.LastName}")
    .Create();
```

### 3. Conditional
```csharp
var dto = order.Builder()
    .MapTo<OrderDto>()
    .SetIf(d => d.Status, o => o.IsPaid, o => "Paid")
    .Create();
```

### 4. Ignore Sensitive Data
```csharp
var dto = user.Builder()
    .MapTo<UserDto>()
    .Ignore(d => d.Password)
    .Create();
```

### 5. With Hooks
```csharp
var dto = source.Builder()
    .MapTo<TargetDto>()
    .BeforeMap((s, d) => Validate(s))
    .Set(d => d.Name, s => s.FullName)
    .AfterMap((s, d) => Log(d))
    .Create();
```

### 6. List Mapping
```csharp
var dtoList = sourceList.FastMapToList<TargetDto>();
```

### 7. Complex Example
```csharp
var dto = order.Builder()
    .MapTo<OrderDto>()
    .BeforeMap((s, d) => Console.WriteLine("Starting..."))
    .Set(d => d.OrderNumber, o => $"#ORD-{o.Id}")
    .Set(d => d.Total, o => (o.Amount + o.Tax).ToString("C"))
    .SetIf(d => d.Status, o => o.IsPaid, o => "✅ Paid")
    .SetIf(d => d.Status, o => !o.IsPaid, o => "⏳ Pending")
    .Ignore(d => d.InternalNotes)
    .AfterMap((s, d) => Console.WriteLine("Done!"))
    .Create();
```

## 🔥 Performance Tips

1. **Use basic `FastMapTo<T>()`** for simple mappings (fastest)
2. **Use Fluent API** only when you need custom logic
3. **Avoid unnecessary `SetIf()`** - use `Set()` when condition is always true
4. **Reuse mapping configurations** when mapping multiple objects

## 📊 When to Use What?

| Scenario | Method | Example |
|----------|--------|---------|
| Simple 1:1 mapping | `FastMapTo<T>()` | `source.FastMapTo<Target>()` |
| Custom calculation | `Set()` | `.Set(d => d.Age, s => Calculate(s))` |
| Conditional logic | `SetIf()` | `.SetIf(d => d.Status, s => s.IsActive, s => "Active")` |
| Priority-based | `SetFirstIfExist()` | `.SetFirstIfExist(d => d.Contact, ...)` |
| Hide sensitive data | `Ignore()` | `.Ignore(d => d.Password)` |
| Validation | `BeforeMap()` | `.BeforeMap((s, d) => Validate(s))` |
| Logging | `AfterMap()` | `.AfterMap((s, d) => Log(d))` |

## 💡 Best Practices

### ✅ DO
```csharp
// Clear and semantic
var dto = user.Builder()
    .MapTo<UserDto>()
    .Set(d => d.FullName, u => $"{u.FirstName} {u.LastName}")
    .Create();
```

### ❌ DON'T
```csharp
// Over-complicated
var dto = user.Builder()
    .MapTo<UserDto>()
    .SetIf(d => d.FirstName, u => true, u => u.FirstName) // Unnecessary condition
    .Create();

// Just use:
var dto = user.FastMapTo<UserDto>(); // or .Set() if needed
```

## 🎨 Real-World Examples

### E-Commerce Order
```csharp
var viewModel = order.Builder()
    .MapTo<OrderViewModel>()
    .Set(vm => vm.OrderNumber, o => $"#ORD-{o.Id}")
    .Set(vm => vm.TotalPrice, o => $"{(o.Amount + o.Tax):C}")
    .SetIf(vm => vm.Status, o => o.IsPaid && o.IsShipped, o => "✅ Delivered")
    .SetIf(vm => vm.Status, o => o.IsPaid && !o.IsShipped, o => "🚚 Shipping")
    .SetIf(vm => vm.Status, o => !o.IsPaid, o => "⏳ Pending Payment")
    .Create();
```

### User Profile (Security)
```csharp
var publicProfile = user.Builder()
    .MapTo<PublicProfileDto>()
    .Ignore(p => p.Email)
    .Ignore(p => p.PhoneNumber)
    .Ignore(p => p.Address)
    .Set(p => p.DisplayName, u => $"@{u.Username}")
    .Create();
```

### API Response Builder
```csharp
var response = data.Builder()
    .MapTo<ApiResponse>()
    .BeforeMap((s, d) => Validate(s))
    .Set(r => r.Timestamp, _ => DateTime.UtcNow)
    .Set(r => r.Status, d => "success")
    .AfterMap((s, d) => LogApiCall(d))
    .Create();
```

## 🔄 Migration from Old API

### Before (Deprecated)
```csharp
var dto = source.Map()
    .MapTo<TargetDto>()
    .Map(d => d.Name, s => s.FullName)
    .MapIf(d => d.Status, s => s.IsActive, s => "Active")
    .To();
```

### After (New API)
```csharp
var dto = source.Builder()
    .MapTo<TargetDto>()
    .Set(d => d.Name, s => s.FullName)
    .SetIf(d => d.Status, s => s.IsActive, s => "Active")
    .Create();
```

**Changes:**
- `Map()` → `Builder()`
- `Map(property, value)` → `Set(property, value)`
- `MapIf()` → `SetIf()`
- `MapIfElse()` → `SetFirstIfExist()`
- `To()` → `Create()`

## 📚 More Examples

See [API_EXAMPLES.md](API_EXAMPLES.md) for comprehensive examples.

---

**FastMapper - Ultra-Performance Object Mapper** 🚀

