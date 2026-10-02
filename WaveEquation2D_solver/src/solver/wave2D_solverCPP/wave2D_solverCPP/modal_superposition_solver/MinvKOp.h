#pragma once

#pragma warning(push)
#pragma warning (disable : 4244)
#pragma warning (disable : 26495)
#pragma warning (disable : 26451)
#pragma warning (disable : 6255)
#pragma warning (disable : 6294)
#pragma warning (disable : 26813)
#pragma warning (disable : 26454)

#include <Eigen/Core>
#include <Eigen/Sparse>
#include <Eigen/SparseCholesky>
#include <Eigen/IterativeLinearSolvers>



#pragma warning(pop)

// Forward declaration or include the necessary headers
class MinvKOp
{
public:
    // Use consistent types - specify Scalar type explicitly
    typedef double Scalar;

    MinvKOp(const Eigen::SparseMatrix<Scalar>& K_,
        Eigen::SimplicialLLT<Eigen::SparseMatrix<Scalar>>& chol_);

    // Required for Eigen's matrix-free solvers
    int rows() const { return m_K.rows(); }
    int cols() const { return m_K.cols(); }

    // Operator application: y = M^{-1} * K * x
    void perform_op(const Scalar* x_in, Scalar* y_out) const;

    // Optional: for compatibility with Eigen's matrix-free framework
    template<typename Derived>
    void apply(const Eigen::MatrixBase<Derived>& x, Eigen::MatrixBase<Derived>& y) const
    {
        perform_op(x.derived().data(), y.derived().data());
    }

private:
    const Eigen::SparseMatrix<Scalar>& m_K;
    Eigen::SimplicialLLT<Eigen::SparseMatrix<Scalar>>& m_chol;
};

