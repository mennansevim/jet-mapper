# Contributing to JetMapper

🎉 First off, thank you for considering contributing to JetMapper!

## How Can You Contribute?

### 🐛 Reporting Bugs

If you find a bug, please open a [GitHub Issue](https://github.com/mennansevim/jet-mapper/issues) and include the following information:

- Clear description of the bug
- Steps to reproduce
- Expected behavior
- Actual behavior
- .NET version and operating system
- Code examples if possible

### ✨ Suggesting Features

If you have a feature suggestion:

1. First check the [GitHub Issues](https://github.com/mennansevim/jet-mapper/issues) page for similar suggestions
2. If none exists, open a new issue and describe the feature in detail
3. Explain why the feature would be useful and how it would be used

### 🔧 Pull Request Process

1. **Fork** the repository and create a new branch:
   ```bash
   git checkout -b feature/amazing-feature
   ```

2. **Make your changes:**
   - Write code that follows the style guidelines (.editorconfig)
   - Add necessary tests
   - Ensure all tests pass

3. **Commit** your changes:
   ```bash
   git commit -m "feat: amazing new feature"
   ```

4. **Push** to your branch:
   ```bash
   git push origin feature/amazing-feature
   ```

5. **Open a Pull Request** and:
   - Describe your changes
   - Reference related issues
   - Confirm that tests pass

## 📝 Code Standards

- **Naming Conventions**: PascalCase (class, method), camelCase (variables, parameters)
- **Code Style**: Follow the .editorconfig file
- **Comments**: Add explanatory comments for complex logic
- **Tests**: Write unit tests for new features
- **Documentation**: Add XML documentation for public APIs

## 🧪 Tests

To run tests:

```bash
dotnet test
```

To run benchmarks:

```bash
cd benchmarks/JetMapper.Benchmarks
dotnet run -c Release
```

## 📚 Commit Messages

Use semantic commit messages:

- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation change
- `style:` - Code formatting (no logic change)
- `refactor:` - Code refactoring
- `test:` - Adding/fixing tests
- `chore:` - Other changes

Examples:
```
feat: add async batch mapping support
fix: null reference exception in FluentMapper
docs: update README with new examples
```

## 🔍 Code Review Process

Your pull request will be reviewed and:

- Code quality will be checked
- Tests will be run
- Performance impacts will be evaluated
- Documentation will be reviewed

## ❓ Questions?

You can use [GitHub Discussions](https://github.com/mennansevim/jet-mapper/discussions) or open an issue.

## 📄 License

By contributing to this project, you agree that your contributions will be licensed under the [MIT License](LICENSE).

---

Thank you for your contributions! 🚀

**Made with ❤️ for the .NET community**
